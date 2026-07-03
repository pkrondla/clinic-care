using HomoeoDesk.Tenant.Domain.Common;
using HomoeoDesk.Tenant.Domain.Entities;
using MediatR;

namespace HomoeoDesk.Tenant.Domain.Modules.Prescriptions.Events
{
    public class PrescriptionCreatedEvent : DomainEvent, INotification
    {
        public Prescription Prescription { get; }

        // Prescription.Id is only assigned once EF Core saves it, which happens
        // after this event is raised — read it lazily so subscribers see the real value.
        public int PrescriptionId => Prescription.Id;

        public PrescriptionCreatedEvent(Prescription prescription)
        {
            Prescription = prescription;
        }
    }
}
