using MediatR;
using HomoeoDesk.Tenant.Application.Common.Models;

namespace HomoeoDesk.Tenant.Application.Features.Appointments.Commands.UpdateAppointment
{
    public record UpdateAppointmentCommand(
        int Id,
        string Notes
    ) : IRequest<Result<AppointmentDto>>;
}

