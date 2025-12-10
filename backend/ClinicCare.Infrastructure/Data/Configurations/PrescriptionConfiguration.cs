using ClinicCare.Domain.Entities;
using ClinicCare.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicCare.Infrastructure.Data.Configurations;

public class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
{
    public void Configure(EntityTypeBuilder<Prescription> builder)
    {
        builder.ToTable("Prescriptions");

        builder.HasKey(x => x.Id);

        // Explicitly map ConsultationId to avoid shadow property
        builder.Property(x => x.ConsultationId)
            .IsRequired();

        builder.Property(x => x.PrescriptionNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.InternalNotes)
            .HasMaxLength(1000);

        builder.Property(x => x.PatientInstructions)
            .HasMaxLength(1000);

        builder.Property(x => x.IssuedDate)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Consultation)
            .WithMany(x => x.Prescriptions)
            .HasForeignKey(x => x.ConsultationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.PrescriptionItems)
            .WithOne(x => x.Prescription)
            .HasForeignKey(x => x.PrescriptionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.PrescriptionNumber)
            .IsUnique()
            .HasDatabaseName("IX_Prescriptions_PrescriptionNumber");
    }
}

public class PrescriptionItemConfiguration : IEntityTypeConfiguration<PrescriptionItem>
{
    public void Configure(EntityTypeBuilder<PrescriptionItem> builder)
    {
        builder.ToTable("PrescriptionItems");

        builder.HasKey(x => x.Id);

        // Explicitly map MedicineId to avoid shadow property
        // MedicineId is nullable to allow custom medicine names without requiring a ClinicMedicine record
        builder.Property(x => x.MedicineId)
            .IsRequired(false)
            .HasColumnName("MedicineId");

        builder.Property(x => x.MedicineName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.DispensingForm)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Dosage)
            .HasMaxLength(100);

        builder.Property(x => x.Frequency)
            .HasMaxLength(100);

        builder.Property(x => x.Duration)
            .HasMaxLength(100);

        builder.Property(x => x.Timing)
            .HasMaxLength(100);

        builder.Property(x => x.ContainerSize)
            .IsRequired(false);

        builder.Property(x => x.Quantity)
            .IsRequired(false); // Required for all forms (prescribed quantity for patient)

        builder.Property(x => x.DispensedQuantity)
            .HasColumnType("decimal(18,2)")
            .IsRequired()
            .HasDefaultValue(0); // Internal: quantity dispensed from inventory (default 0 if column doesn't exist yet)

        builder.Property(x => x.UnitPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.TotalPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.Instructions)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Prescription)
            .WithMany(x => x.PrescriptionItems)
            .HasForeignKey(x => x.PrescriptionId)
            .OnDelete(DeleteBehavior.Cascade);

        // CRITICAL: MedicineId has a foreign key in the database (FK_PrescriptionItems_Medicine)
        // but we don't have a navigation property in the entity
        // Tell EF Core that MedicineId is a foreign key column but don't configure a relationship
        // This prevents shadow property creation
        builder.HasIndex(x => x.MedicineId)
            .HasDatabaseName("IX_PrescriptionItems_MedicineId");
    }
}

