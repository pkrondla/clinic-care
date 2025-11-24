using ClinicCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicCare.Infrastructure.Data.Configurations;

public class ConsultationConfiguration : IEntityTypeConfiguration<Consultation>
{
    public void Configure(EntityTypeBuilder<Consultation> builder)
    {
        builder.ToTable("Consultations");

        builder.HasKey(x => x.Id);

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
        // Explicitly map AppointmentId property to avoid shadow property
        builder.Property(x => x.AppointmentId)
            .IsRequired();
        builder.HasOne(x => x.Appointment)
            .WithOne(x => x.Consultation)
            .HasForeignKey<Consultation>(x => x.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure many-to-one relationship with Organization
        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure many-to-one relationship with Doctor
        builder.HasOne(x => x.Doctor)
            .WithMany()
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure many-to-one relationship with Patient
        // Explicitly map PatientId property to avoid shadow property
        builder.Property(x => x.PatientId)
            .IsRequired();
        builder.HasOne(x => x.Patient)
            .WithMany()
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

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

