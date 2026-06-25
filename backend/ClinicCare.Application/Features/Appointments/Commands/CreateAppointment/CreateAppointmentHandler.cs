using MediatR;
using AutoMapper;
using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Common.Services;
using ClinicCare.Domain.Modules.Appointments.Entities;
using ClinicCare.Domain.Modules.Appointments.ValueObjects;
using ClinicCare.Domain.Enums;
using ClinicCare.Application.Features.Appointments.Queries.GetAppointments;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.Appointments.Commands.CreateAppointment
{
    public class CreateAppointmentHandler : IRequestHandler<CreateAppointmentCommand, Result<AppointmentDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly ITokenNumberService _tokenNumberService;
        private readonly IQueueNotificationService _queueNotificationService;
        private readonly INotificationService _notificationService;

        public CreateAppointmentHandler(
            IApplicationDbContext context, 
            IAppointmentRepository appointmentRepository, 
            ICurrentUserService currentUserService, 
            IMapper mapper,
            ITokenNumberService tokenNumberService,
            IQueueNotificationService queueNotificationService,
            INotificationService notificationService)
        {
            _context = context;
            _appointmentRepository = appointmentRepository;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _tokenNumberService = tokenNumberService;
            _queueNotificationService = queueNotificationService;
            _notificationService = notificationService;
        }

        public async Task<Result<AppointmentDto>> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Validate business rules
                var validationResult = await ValidateAppointmentCreation(request, cancellationToken);
                if (!validationResult.Succeeded)
                    return Result<AppointmentDto>.Failure(validationResult.Errors);

                // 2. Auto-generate token number if not provided
                var tokenNumber = request.TokenNumber ?? 
                    await _tokenNumberService.GetNextTokenNumberAsync(
                        request.DoctorId, 
                        request.ClinicId, 
                        request.AppointmentDate, 
                        cancellationToken);

                // 3. Create domain entity using Appointment.Create (since TokenNumber has private setter)
                var organizationId = _currentUserService.OrganizationId ?? throw new UnauthorizedAccessException("User not associated with any organization");
                var appointmentDate = AppointmentDate.Create(request.AppointmentDate);
                var appointmentType = (AppointmentType)request.Type;
                
                var appointment = Appointment.Create(
                    organizationId,
                    request.ClinicId,
                    request.DoctorId,
                    request.PatientId,
                    appointmentDate,
                    tokenNumber,
                    appointmentType,
                    request.Notes ?? string.Empty
                );

                // 3. Save to database using repository
                await _appointmentRepository.AddAsync(appointment, cancellationToken);

                // 4. Load related data for response using repository
                var appointmentWithDetails = await _appointmentRepository.GetByIdWithDetailsAsync(appointment.Id, cancellationToken);

                // 5. Broadcast queue update via SignalR
                await _queueNotificationService.BroadcastQueueUpdateAsync(
                    organizationId,
                    request.ClinicId,
                    request.DoctorId,
                    cancellationToken);

                // 6. Return DTO using AutoMapper
                var dto = _mapper.Map<ClinicCare.Application.Features.Appointments.Queries.GetAppointments.AppointmentDto>(appointmentWithDetails);
                return Result<AppointmentDto>.Success(new AppointmentDto
                {
                    Id = dto.Id,
                    ClinicId = appointmentWithDetails!.ClinicId,
                    DoctorId = appointmentWithDetails.DoctorId,
                    PatientId = appointmentWithDetails.PatientId,
                    AppointmentDate = dto.AppointmentDate,
                    TokenNumber = dto.TokenNumber,
                    Type = dto.Type,
                    Status = dto.Status,
                    Notes = dto.Notes,
                    DoctorName = dto.Doctor.Name,
                    PatientName = dto.Patient.Name,
                    ClinicName = dto.Clinic.Name
                });
            }
            catch (Exception ex)
            {
                return Result<AppointmentDto>.Failure(new[] { ex.Message });
            }
        }

        private async Task<Result<bool>> ValidateAppointmentCreation(CreateAppointmentCommand request, CancellationToken cancellationToken)
        {
            // Check if doctor exists and is available
            var doctorExists = await _context.DoctorProfiles
                .AnyAsync(x => x.Id == request.DoctorId && x.OrganizationId == _currentUserService.OrganizationId, cancellationToken);

            if (!doctorExists)
                return Result<bool>.Failure(new[] { "Doctor not found" });

            // Check if patient exists
            var patientExists = await _context.Patients
                .AnyAsync(x => x.Id == request.PatientId && x.OrganizationId == _currentUserService.OrganizationId, cancellationToken);

            if (!patientExists)
                return Result<bool>.Failure(new[] { "Patient not found" });

            // Check if clinic exists
            var clinicExists = await _context.Clinics
                .AnyAsync(x => x.Id == request.ClinicId && x.OrganizationId == _currentUserService.OrganizationId, cancellationToken);

            if (!clinicExists)
                return Result<bool>.Failure(new[] { "Clinic not found" });

            // Note: Token number conflict check is not needed since we auto-generate sequential tokens
            // If token number is provided manually, we still validate it
            if (request.TokenNumber.HasValue)
            {
                var hasConflict = await _appointmentRepository.HasConflictingAppointmentAsync(
                    request.DoctorId, 
                    request.ClinicId, 
                    request.AppointmentDate, 
                    request.TokenNumber.Value, 
                    null, 
                    cancellationToken);

                if (hasConflict)
                    return Result<bool>.Failure(new[] { "Token number is already taken for this doctor on this date" });
            }

            return Result<bool>.Success(true);
        }
    }
}
