using ClinicCare.Domain.Common;
using MediatR;

namespace ClinicCare.Domain.Modules.Appointments.Events
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
