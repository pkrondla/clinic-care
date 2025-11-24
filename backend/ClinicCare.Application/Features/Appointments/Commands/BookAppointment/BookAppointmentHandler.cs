using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Common.Services;
using ClinicCare.Application.Features.Appointments.Queries.GetAppointments;
using ClinicCare.Domain.Modules.Appointments.Entities;
using ClinicCare.Domain.Modules.Appointments.ValueObjects;
using ClinicCare.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.Appointments.Commands.BookAppointment;

public class BookAppointmentHandler : IRequestHandler<BookAppointmentCommand, Result<AppointmentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITokenNumberService _tokenNumberService;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IQueueNotificationService _queueNotificationService;

    public BookAppointmentHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ITokenNumberService tokenNumberService,
        IAppointmentRepository appointmentRepository,
        IQueueNotificationService queueNotificationService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _tokenNumberService = tokenNumberService;
        _appointmentRepository = appointmentRepository;
        _queueNotificationService = queueNotificationService;
    }

    public async Task<Result<AppointmentDto>> Handle(BookAppointmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            var userId = _currentUserService.UserId;

            if (!organizationId.HasValue)
            {
                return Result<AppointmentDto>.Failure("User not associated with any organization");
            }

            if (!userId.HasValue)
            {
                return Result<AppointmentDto>.Failure("User not authenticated");
            }

            // Get patient associated with this user
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId.Value 
                                       && p.OrganizationId == organizationId.Value 
                                       && p.IsActive, cancellationToken);

            if (patient == null)
            {
                return Result<AppointmentDto>.Failure("Patient profile not found. Please contact clinic staff.");
            }

            // Validate doctor exists
            var doctorExists = await _context.DoctorProfiles
                .AnyAsync(d => d.Id == request.DoctorId 
                            && d.OrganizationId == organizationId.Value 
                            && d.IsActive, cancellationToken);

            if (!doctorExists)
            {
                return Result<AppointmentDto>.Failure("Doctor not found");
            }

            // Validate clinic exists
            var clinicExists = await _context.Clinics
                .AnyAsync(c => c.Id == request.ClinicId 
                            && c.OrganizationId == organizationId.Value 
                            && c.IsActive, cancellationToken);

            if (!clinicExists)
            {
                return Result<AppointmentDto>.Failure("Clinic not found");
            }

            // Auto-generate token number
            var tokenNumber = await _tokenNumberService.GetNextTokenNumberAsync(
                request.DoctorId,
                request.ClinicId,
                request.AppointmentDate,
                cancellationToken);

            // Create appointment using Appointment.Create
            var appointment = Appointment.Create(
                organizationId.Value,
                request.ClinicId,
                request.DoctorId,
                patient.Id,
                AppointmentDate.Create(request.AppointmentDate),
                tokenNumber,
                (AppointmentType)request.Type,
                request.Notes
            );

            await _appointmentRepository.AddAsync(appointment, cancellationToken);

            // Load with details
            var appointmentWithDetails = await _appointmentRepository.GetByIdWithDetailsAsync(appointment.Id, cancellationToken);

            if (appointmentWithDetails == null)
            {
                return Result<AppointmentDto>.Failure("Failed to retrieve created appointment");
            }

            // Broadcast queue update
            await _queueNotificationService.BroadcastQueueUpdateAsync(
                organizationId.Value,
                request.ClinicId,
                request.DoctorId,
                cancellationToken);

            var dto = new AppointmentDto
            {
                Id = appointmentWithDetails.Id,
                ClinicId = appointmentWithDetails.ClinicId,
                DoctorId = appointmentWithDetails.DoctorId,
                PatientId = appointmentWithDetails.PatientId,
                AppointmentDate = appointmentWithDetails.AppointmentDate.Value,
                TokenNumber = appointmentWithDetails.TokenNumber,
                Type = (int)appointmentWithDetails.Type,
                Status = (int)appointmentWithDetails.Status,
                Notes = appointmentWithDetails.Notes,
                DoctorName = appointmentWithDetails.Doctor.User?.FullName ?? "Unknown",
                PatientName = appointmentWithDetails.Patient.User?.FullName ?? "Unknown",
                ClinicName = appointmentWithDetails.Clinic.Name
            };

            return Result<AppointmentDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<AppointmentDto>.Failure($"Failed to book appointment: {ex.Message}");
        }
    }
}

