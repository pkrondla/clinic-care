using HomoeoDesk.Tenant.Domain.Common;
using HomoeoDesk.Tenant.Domain.Enums;
using HomoeoDesk.Tenant.Domain.Entities;
using HomoeoDesk.Tenant.Domain.Modules.Appointments.ValueObjects;
using HomoeoDesk.Tenant.Domain.Modules.Appointments.Events;

namespace HomoeoDesk.Tenant.Domain.Modules.Appointments.Entities
{
    public class Appointment : TenantEntity
    {
        public int BranchId { get; private set; }
        public int DoctorId { get; private set; }
        public int PatientId { get; private set; }
        public AppointmentDate AppointmentDate { get; private set; }
        public int TokenNumber { get; private set; }
        public AppointmentType Type { get; private set; }
        public AppointmentStatus Status { get; private set; }
        public string Notes { get; private set; } = string.Empty;

        // Navigation Properties
        public Branch Branch { get; set; } = null!;
        public DoctorProfile Doctor { get; set; } = null!;
        public Patient Patient { get; set; } = null!;
        public Consultation? Consultation { get; set; }

        private Appointment() { } // EF Core

        public static Appointment Create(
            int organizationId,
            int BranchId,
            int doctorId,
            int patientId,
            AppointmentDate appointmentDate,
            int tokenNumber,
            AppointmentType type,
            string notes = "")
        {
            var appointment = new Appointment
            {
                OrganizationId = organizationId,
                BranchId = BranchId,
                DoctorId = doctorId,
                PatientId = patientId,
                AppointmentDate = appointmentDate,
                TokenNumber = tokenNumber,
                Type = type,
                Status = AppointmentStatus.Scheduled,
                Notes = notes
            };

            appointment.AddDomainEvent(new AppointmentCreatedEvent(appointment.Id));
            return appointment;
        }

        public void UpdateNotes(string notes)
        {
            if (Status == AppointmentStatus.Completed)
                throw new InvalidOperationException("Cannot update notes for completed appointment");

            Notes = notes;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new AppointmentUpdatedEvent(Id));
        }

        public void Start()
        {
            if (Status != AppointmentStatus.Scheduled)
                throw new InvalidOperationException("Only scheduled appointments can be started");

            Status = AppointmentStatus.InProgress;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new AppointmentUpdatedEvent(Id));
        }

        public void Complete()
        {
            if (Status != AppointmentStatus.InProgress)
                throw new InvalidOperationException("Only in-progress appointments can be completed");

            Status = AppointmentStatus.Completed;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new AppointmentUpdatedEvent(Id));
        }

        public void Cancel()
        {
            if (Status == AppointmentStatus.Completed)
                throw new InvalidOperationException("Cannot cancel completed appointment");

            Status = AppointmentStatus.Cancelled;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new AppointmentCancelledEvent(Id));
        }

        public void Reschedule(AppointmentDate newDate, int newTokenNumber)
        {
            if (Status == AppointmentStatus.Completed)
                throw new InvalidOperationException("Cannot reschedule completed appointment");

            AppointmentDate = newDate;
            TokenNumber = newTokenNumber;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new AppointmentUpdatedEvent(Id));
        }
    }
}
