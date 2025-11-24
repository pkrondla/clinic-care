using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Common.Services;
using ClinicCare.Application.Features.Appointments.Queries.GetAppointments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.Appointments.Commands.CompleteAppointment;

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
                .Include(a => a.Clinic)
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
                    appointment.ClinicId,
                    appointment.DoctorId,
                    cancellationToken);
            }

            var dto = new AppointmentDto
            {
                Id = appointment.Id,
                ClinicId = appointment.ClinicId,
                DoctorId = appointment.DoctorId,
                PatientId = appointment.PatientId,
                AppointmentDate = appointment.AppointmentDate.Value,
                TokenNumber = appointment.TokenNumber,
                Type = (int)appointment.Type,
                Status = (int)appointment.Status,
                Notes = appointment.Notes,
                DoctorName = appointment.Doctor.User?.FullName ?? "Unknown",
                PatientName = appointment.Patient.User?.FullName ?? "Unknown",
                ClinicName = appointment.Clinic.Name
            };

            return Result<AppointmentDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<AppointmentDto>.Failure($"Failed to complete appointment: {ex.Message}");
        }
    }
}

