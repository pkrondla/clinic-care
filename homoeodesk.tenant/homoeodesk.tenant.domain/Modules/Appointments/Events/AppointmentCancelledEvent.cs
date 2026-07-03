using HomoeoDesk.Tenant.Domain.Common;
using MediatR;

namespace HomoeoDesk.Tenant.Domain.Modules.Appointments.Events
{
    public class AppointmentCancelledEvent : DomainEvent, INotification
    {
        public int AppointmentId { get; }

        public AppointmentCancelledEvent(int appointmentId)
        {
            AppointmentId = appointmentId;
        }
    }
}
