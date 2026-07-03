using HomoeoDesk.Tenant.Domain.Common;
using HomoeoDesk.Tenant.Domain.Modules.Appointments.Entities;
using MediatR;

namespace HomoeoDesk.Tenant.Domain.Modules.Appointments.Events
{
    public class AppointmentCreatedEvent : DomainEvent, INotification
    {
        public Appointment Appointment { get; }

        // Appointment.Id is only assigned once EF Core saves it, which happens
        // after this event is raised — read it lazily so subscribers see the real value.
        public int AppointmentId => Appointment.Id;

        public AppointmentCreatedEvent(Appointment appointment)
        {
            Appointment = appointment;
        }
    }
}
