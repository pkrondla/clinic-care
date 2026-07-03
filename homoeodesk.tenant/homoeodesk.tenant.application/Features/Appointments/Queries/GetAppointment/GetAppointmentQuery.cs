using MediatR;
using HomoeoDesk.Tenant.Application.Common.Models;

namespace HomoeoDesk.Tenant.Application.Features.Appointments.Queries.GetAppointment
{
    public record GetAppointmentQuery(int Id) : IRequest<Result<AppointmentDto>>;
}

