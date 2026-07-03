using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Common.Services;
using HomoeoDesk.Tenant.Application.Features.Appointments.Queries.GetAppointments;
using HomoeoDesk.Tenant.Domain.Modules.Appointments.Entities;
using HomoeoDesk.Tenant.Domain.Modules.Appointments.ValueObjects;
using HomoeoDesk.Tenant.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Appointments.Commands.BookAppointment;

public class BookAppointmentHandler : IRequestHandler<BookAppointmentCommand, Result<AppointmentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITokenNumberService _tokenNumberService;
    private readonly IQueueNotificationService _queueNotificationService;

    public BookAppointmentHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ITokenNumberService tokenNumberService,
        IQueueNotificationService queueNotificationService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _tokenNumberService = tokenNumberService;
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
            var clinicExists = await _context.Branches
                .AnyAsync(c => c.Id == request.BranchId 
                            && c.OrganizationId == organizationId.Value 
                            && c.IsActive, cancellationToken);

            if (!clinicExists)
            {
                return Result<AppointmentDto>.Failure("Branch not found");
            }

            // Auto-generate token number
            var tokenNumber = await _tokenNumberService.GetNextTokenNumberAsync(
                request.DoctorId,
                request.BranchId,
                request.AppointmentDate,
                cancellationToken);

            // Create appointment using Appointment.Create
            var appointment = Appointment.Create(
                organizationId.Value,
                request.BranchId,
                request.DoctorId,
                patient.Id,
                AppointmentDate.Create(request.AppointmentDate),
                tokenNumber,
                (AppointmentType)request.Type,
                request.Notes
            );

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync(cancellationToken);

            var appointmentWithDetails = await _context.Appointments
                .Include(x => x.Doctor)
                    .ThenInclude(x => x.User)
                .Include(x => x.Patient)
                    .ThenInclude(x => x.User)
                .Include(x => x.Branch)
                .Include(x => x.Consultation)
                .FirstOrDefaultAsync(x => x.Id == appointment.Id, cancellationToken);

            if (appointmentWithDetails == null)
            {
                return Result<AppointmentDto>.Failure("Failed to retrieve created appointment");
            }

            // Broadcast queue update
            await _queueNotificationService.BroadcastQueueUpdateAsync(
                organizationId.Value,
                request.BranchId,
                request.DoctorId,
                cancellationToken);

            var dto = new AppointmentDto
            {
                Id = appointmentWithDetails.Id,
                AppointmentDate = appointmentWithDetails.AppointmentDate.Value,
                TokenNumber = appointmentWithDetails.TokenNumber,
                Type = (int)appointmentWithDetails.Type,
                Status = (int)appointmentWithDetails.Status,
                Notes = appointmentWithDetails.Notes,
                Doctor = new DoctorDto
                {
                    Id = appointmentWithDetails.Doctor.Id,
                    Name = appointmentWithDetails.Doctor.User?.FullName ?? "Unknown",
                    Qualification = appointmentWithDetails.Doctor.Qualification
                },
                Patient = new PatientDto
                {
                    Id = appointmentWithDetails.Patient.Id,
                    Name = appointmentWithDetails.Patient.User?.FullName ?? "Unknown",
                    PatientCode = appointmentWithDetails.Patient.PatientCode
                },
                Branch = new BranchDto
                {
                    Id = appointmentWithDetails.Branch.Id,
                    Name = appointmentWithDetails.Branch.Name,
                    Code = appointmentWithDetails.Branch.Code
                }
            };

            return Result<AppointmentDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<AppointmentDto>.Failure($"Failed to book appointment: {ex.Message}");
        }
    }
}

