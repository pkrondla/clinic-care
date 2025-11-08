using MediatR;
using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Models;
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
    }
}

