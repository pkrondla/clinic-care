using ClinicCare.Domain.Common;
using MediatR;

namespace ClinicCare.Domain.Modules.Appointments.Events
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
