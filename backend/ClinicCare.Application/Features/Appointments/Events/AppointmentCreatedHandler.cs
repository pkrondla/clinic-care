using MediatR;
using ClinicCare.Domain.Modules.Appointments.Events;

namespace ClinicCare.Application.Features.Appointments.Events
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

