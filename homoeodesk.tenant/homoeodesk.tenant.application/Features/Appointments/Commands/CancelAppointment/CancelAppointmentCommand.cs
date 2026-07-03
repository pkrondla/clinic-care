using MediatR;
using HomoeoDesk.Tenant.Application.Common.Models;

namespace HomoeoDesk.Tenant.Application.Features.Appointments.Commands.CancelAppointment
{
    public record CancelAppointmentCommand(int Id) : IRequest<Result<bool>>;
}

