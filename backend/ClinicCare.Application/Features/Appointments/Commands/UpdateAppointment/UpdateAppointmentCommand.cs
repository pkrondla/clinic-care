using MediatR;
using ClinicCare.Application.Common.Models;

namespace ClinicCare.Application.Features.Appointments.Commands.UpdateAppointment
{
    public record UpdateAppointmentCommand(
        int Id,
        string Notes
    ) : IRequest<Result<AppointmentDto>>;
}

