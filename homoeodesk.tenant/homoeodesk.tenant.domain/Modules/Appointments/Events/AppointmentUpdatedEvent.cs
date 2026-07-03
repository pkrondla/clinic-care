using HomoeoDesk.Tenant.Domain.Common;
using MediatR;

namespace HomoeoDesk.Tenant.Domain.Modules.Appointments.Events
{
    public class AppointmentUpdatedEvent : DomainEvent, INotification
    {
        public int AppointmentId { get; }

        public AppointmentUpdatedEvent(int appointmentId)
        {
            AppointmentId = appointmentId;
        }
    }
}
