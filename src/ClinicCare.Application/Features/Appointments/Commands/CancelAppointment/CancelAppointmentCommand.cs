using MediatR;
using ClinicCare.Application.Common.Models;

namespace ClinicCare.Application.Features.Appointments.Commands.CancelAppointment
{
    public record CancelAppointmentCommand(int Id) : IRequest<Result<bool>>;
}

