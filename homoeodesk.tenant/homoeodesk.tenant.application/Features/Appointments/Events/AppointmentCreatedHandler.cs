using MediatR;
using HomoeoDesk.Tenant.Domain.Modules.Appointments.Events;

namespace HomoeoDesk.Tenant.Application.Features.Appointments.Events
{
    public class AppointmentCreatedHandler : INotificationHandler<AppointmentCreatedEvent>
    {
        public async Task Handle(AppointmentCreatedEvent notification, CancellationToken cancellationToken)
        {
            // Handle appointment created event
            // This could trigger notifications, billing, etc.
            await Task.CompletedTask;
        }
    }
}

