using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Common.Services;
using ClinicCare.Application.Features.Appointments.Queries.GetAppointments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.Appointments.Commands.StartAppointment;

public class StartAppointmentHandler : IRequestHandler<StartAppointmentCommand, Result<AppointmentDto>>
{
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IQueueNotificationService _queueNotificationService;

        public StartAppointmentHandler(
            IApplicationDbContext context, 
            ICurrentUserService currentUserService,
            IQueueNotificationService queueNotificationService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _queueNotificationService = queueNotificationService;
        }

    public async Task<Result<AppointmentDto>> Handle(StartAppointmentCommand request, CancellationToken cancellationToken)
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

            // Use domain method to start appointment
            appointment.Start();

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
                Clinic = new ClinicDto
                {
                    Id = appointment.Clinic.Id,
                    Name = appointment.Clinic.Name,
                    Code = appointment.Clinic.Code
                }
            };

            return Result<AppointmentDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<AppointmentDto>.Failure($"Failed to start appointment: {ex.Message}");
        }
    }
}

