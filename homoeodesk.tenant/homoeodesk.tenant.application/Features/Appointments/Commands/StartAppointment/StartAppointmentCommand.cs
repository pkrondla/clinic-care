using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Appointments.Queries.GetAppointments;
using MediatR;

namespace HomoeoDesk.Tenant.Application.Features.Appointments.Commands.StartAppointment;

public class StartAppointmentCommand : IRequest<Result<AppointmentDto>>
{
    public int Id { get; set; }
}

