using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Common.Services;
using HomoeoDesk.Tenant.Application.Features.Appointments.Queries.GetAppointments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Appointments.Commands.CompleteAppointment;

public class CompleteAppointmentHandler : IRequestHandler<CompleteAppointmentCommand, Result<AppointmentDto>>
{
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IQueueNotificationService _queueNotificationService;

        public CompleteAppointmentHandler(
            IApplicationDbContext context, 
            ICurrentUserService currentUserService,
            IQueueNotificationService queueNotificationService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _queueNotificationService = queueNotificationService;
        }

    public async Task<Result<AppointmentDto>> Handle(CompleteAppointmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Branch)
                .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

            if (appointment == null)
            {
                return Result<AppointmentDto>.Failure("Appointment not found");
            }

            // Use domain method to complete appointment
            appointment.Complete();

            await _context.SaveChangesAsync(cancellationToken);

            // Broadcast queue update
            var organizationId = _currentUserService.OrganizationId;
            if (organizationId.HasValue)
            {
                await _queueNotificationService.BroadcastQueueUpdateAsync(
                    organizationId.Value,
                    appointment.BranchId,
                    appointment.DoctorId,
                    cancellationToken);
            }

            var dto = new AppointmentDto
            {
                Id = appointment.Id,
                AppointmentDate = appointment.AppointmentDate.Value,
                TokenNumber = appointment.TokenNumber,
                Type = (int)appointment.Type,
                Status = (int)appointment.Status,
                Notes = appointment.Notes,
                Doctor = new DoctorDto
                {
                    Id = appointment.Doctor.Id,
                    Name = appointment.Doctor.User?.FullName ?? "Unknown",
                    Qualification = appointment.Doctor.Qualification
                },
                Patient = new PatientDto
                {
                    Id = appointment.Patient.Id,
                    Name = appointment.Patient.User?.FullName ?? "Unknown",
                    PatientCode = appointment.Patient.PatientCode
                },
                Branch = new BranchDto
                {
                    Id = appointment.Branch.Id,
                    Name = appointment.Branch.Name,
                    Code = appointment.Branch.Code
                }
            };

            return Result<AppointmentDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<AppointmentDto>.Failure($"Failed to complete appointment: {ex.Message}");
        }
    }
}

