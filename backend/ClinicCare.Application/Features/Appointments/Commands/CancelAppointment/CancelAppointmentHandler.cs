using MediatR;
using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Models;
using ClinicCare.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.Appointments.Commands.CancelAppointment
{
    public class CancelAppointmentHandler : IRequestHandler<CancelAppointmentCommand, Result<bool>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public CancelAppointmentHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Result<bool>> Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var appointment = await _context.Appointments
                    .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

                if (appointment == null)
                    return Result<bool>.Failure(new[] { "Appointment not found" });

                // Check if user has permission to cancel this appointment
                var permissionResult = await CheckCancelPermission(appointment, cancellationToken);
                if (!permissionResult.Succeeded)
                    return permissionResult;

                // Cancel appointment using domain method
                appointment.Cancel();

                await _context.SaveChangesAsync(cancellationToken);

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure(new[] { ex.Message });
            }
        }

        private async Task<Result<bool>> CheckCancelPermission(
            Domain.Modules.Appointments.Entities.Appointment appointment, 
            CancellationToken cancellationToken)
        {
            var role = _currentUserService.Role;
            var userId = _currentUserService.UserId;

            if (!role.HasValue || !userId.HasValue)
                return Result<bool>.Failure(new[] { "User not authenticated" });

            // Admin and Staff can cancel any appointment
            if (role == UserRole.Admin || role == UserRole.SuperAdmin || role == UserRole.Staff)
            {
                return Result<bool>.Success(true);
            }

            // Doctor can cancel their own appointments
            if (role == UserRole.Doctor)
            {
                var doctorProfile = await _context.DoctorProfiles
                    .FirstOrDefaultAsync(x => x.UserId == userId.Value, cancellationToken);

                if (doctorProfile == null)
                    return Result<bool>.Failure(new[] { "Doctor profile not found" });

                if (appointment.DoctorId == doctorProfile.Id)
                {
                    return Result<bool>.Success(true);
                }

                return Result<bool>.Failure(new[] { "You can only cancel your own appointments" });
            }

            // Patient can cancel their own appointments
            if (role == UserRole.Patient)
            {
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(x => x.UserId == userId.Value, cancellationToken);

                if (patient == null)
                    return Result<bool>.Failure(new[] { "Patient profile not found" });

                if (appointment.PatientId == patient.Id)
                {
                    return Result<bool>.Success(true);
                }

                return Result<bool>.Failure(new[] { "You can only cancel your own appointments" });
            }

            // Unknown role
            return Result<bool>.Failure(new[] { "You do not have permission to cancel appointments" });
        }
    }
}

