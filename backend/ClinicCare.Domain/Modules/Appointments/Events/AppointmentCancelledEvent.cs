using ClinicCare.Domain.Common;
using MediatR;

namespace ClinicCare.Domain.Modules.Appointments.Events
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
