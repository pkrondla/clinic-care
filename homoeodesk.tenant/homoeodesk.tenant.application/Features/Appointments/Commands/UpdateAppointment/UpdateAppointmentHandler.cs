using MediatR;
using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Appointments.Commands.UpdateAppointment
{
    public class UpdateAppointmentHandler : IRequestHandler<UpdateAppointmentCommand, Result<AppointmentDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public UpdateAppointmentHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Result<AppointmentDto>> Handle(UpdateAppointmentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var appointment = await _context.Appointments
                    .Include(x => x.Doctor)
                    .ThenInclude(x => x.User)
                    .Include(x => x.Patient)
                    .ThenInclude(x => x.User)
                    .Include(x => x.Branch)
                    .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

                if (appointment == null)
                    return Result<AppointmentDto>.Failure(new[] { "Appointment not found" });

                // Update notes using domain method
                appointment.UpdateNotes(request.Notes);

                await _context.SaveChangesAsync(cancellationToken);

                var dto = new AppointmentDto
                {
                    Id = appointment.Id,
                    BranchId = appointment.BranchId,
                    DoctorId = appointment.DoctorId,
                    PatientId = appointment.PatientId,
                    AppointmentDate = appointment.AppointmentDate.Value,
                    TokenNumber = appointment.TokenNumber,
                    Type = (int)appointment.Type,
                    Status = (int)appointment.Status,
                    Notes = appointment.Notes,
                    DoctorName = appointment.Doctor.User.FullName,
                    PatientName = appointment.Patient.User.FullName,
                    BranchName = appointment.Branch.Name
                };

                return Result<AppointmentDto>.Success(dto);
            }
            catch (Exception ex)
            {
                return Result<AppointmentDto>.Failure(new[] { ex.Message });
            }
        }
    }
}

