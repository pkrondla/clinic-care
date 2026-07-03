using HomoeoDesk.Tenant.Domain.Common;
using MediatR;

namespace HomoeoDesk.Tenant.Domain.Modules.Appointments.Events
{
    public class AppointmentCreatedEvent : DomainEvent, INotification
    {
        public int AppointmentId { get; }

        public AppointmentCreatedEvent(int appointmentId)
        {
            AppointmentId = appointmentId;
        }
    }
}
