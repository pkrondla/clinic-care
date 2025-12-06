using ClinicCare.Domain.Entities;
using ClinicCare.Domain.Modules.Appointments.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicCare.Infrastructure.Data.Configurations;

public class ConsultationConfiguration : IEntityTypeConfiguration<Consultation>
{
    public void Configure(EntityTypeBuilder<Consultation> builder)
    {
        builder.ToTable("Consultations");

        builder.HasKey(x => x.Id);

        // CRITICAL: Explicitly map foreign key properties BEFORE relationships to avoid shadow properties
        // Database column: AppointmentId (NOT AppointmentId1, AppointmentId2, etc.)
        // We use HasColumnName to explicitly map to the database column
        // This tells EF Core exactly which column to use and prevents it from creating shadow properties
        builder.Property(x => x.AppointmentId)
            .HasColumnName("AppointmentId")
            .IsRequired();

        builder.Property(x => x.DoctorId)
            .IsRequired();

        builder.Property(x => x.PatientId)
            .IsRequired();

        builder.Property(x => x.ChiefComplaint)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Symptoms)
            .HasMaxLength(1000);

        builder.Property(x => x.Examination)
            .HasMaxLength(1000);

        builder.Property(x => x.Diagnosis)
            .HasMaxLength(500);

        builder.Property(x => x.TreatmentPlan)
            .HasMaxLength(1000);

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        builder.Property(x => x.ConsultationFee)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.ConsultationDate)
            .IsRequired();

        // Configure one-to-one relationship with Appointment
        // Database schema: FK_Consultations_Appointment (Consultations.AppointmentId -> Appointments.Id)
        // CRITICAL: The foreign key is ONLY on the Consultation side (AppointmentId)
        // Appointment does NOT have a foreign key - it only has a navigation property (Consultation)
        // IMPORTANT: AppointmentId property is already mapped above with explicit column name
        // Consultation.Appointment is of type Domain.Modules.Appointments.Entities.Appointment
        // We let EF Core infer the type from the navigation property to avoid type mismatches
        // This prevents EF Core from creating shadow properties like AppointmentId1, AppointmentId2
        builder.HasOne(x => x.Appointment)
            .WithOne(a => a.Consultation)
            .HasForeignKey<Consultation>(x => x.AppointmentId)
            .HasPrincipalKey<Appointment>(a => a.Id)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        
        // CRITICAL: Ensure no duplicate foreign key configurations
        // Remove any other Appointment-related configurations that might create shadow properties
        // This is necessary because we have two Appointment types in the codebase

        // Configure many-to-one relationship with Organization
        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure many-to-one relationship with Doctor
        // Doctor relationship - explicitly specify the navigation property on DoctorProfile
        // This prevents EF Core from creating shadow properties
        builder.HasOne(x => x.Doctor)
            .WithMany(d => d.Consultations)
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Configure many-to-one relationship with Patient
        // Patient relationship - explicitly specify the navigation property on Patient
        // This prevents EF Core from creating shadow properties
        builder.HasOne(x => x.Patient)
            .WithMany(p => p.Consultations)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Configure one-to-many relationship with Prescriptions
        // Note: Prescription.ConsultationId foreign key is configured in PrescriptionConfiguration
        builder.HasMany(x => x.Prescriptions)
            .WithOne(x => x.Consultation)
            .HasForeignKey(x => x.ConsultationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure one-to-one relationship with Invoice
        // Note: Invoice.ConsultationId foreign key is configured in InvoiceConfiguration
        builder.HasOne(x => x.Invoice)
            .WithOne(x => x.Consultation)
            .HasForeignKey<Invoice>(x => x.ConsultationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure indexes
        builder.HasIndex(x => new { x.OrganizationId, x.AppointmentId })
            .IsUnique()
            .HasDatabaseName("IX_Consultations_OrganizationAppointment");

        builder.HasIndex(x => new { x.OrganizationId, x.DoctorId, x.ConsultationDate })
            .HasDatabaseName("IX_Consultations_DoctorDate");

        builder.HasIndex(x => new { x.OrganizationId, x.PatientId, x.ConsultationDate })
            .HasDatabaseName("IX_Consultations_PatientDate");
    }
}

