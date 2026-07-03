using HomoeoDesk.Tenant.Domain.Entities;
using HomoeoDesk.Tenant.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomoeoDesk.Tenant.Infrastructure.Data.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");

        builder.HasKey(x => x.Id);

        // Explicitly map foreign key properties BEFORE relationships to avoid shadow properties
        builder.Property(x => x.ConsultationId)
            .IsRequired(false);

        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.Property(x => x.PatientId)
            .IsRequired();

        builder.Property(x => x.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.ConsultationAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.MedicineAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.CourierCharges)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.TotalAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.PaidAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.BalanceAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.PaymentMethod)
            .HasMaxLength(50);

        builder.Property(x => x.PaymentReference)
            .HasMaxLength(100);

        builder.Property(x => x.InvoiceDate)
            .IsRequired();

        // PrescriptionId property
        builder.Property(x => x.PrescriptionId)
            .IsRequired(false);

        // Courier fields
        builder.Property(x => x.CourierDocketNumber)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(x => x.CourierCompany)
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(x => x.CourierDispatchedDate)
            .IsRequired(false);

        builder.Property(x => x.CourierTrackingUrl)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(x => x.CourierStatus)
            .HasConversion<int?>()
            .IsRequired(false);

        // Relationships - Configure explicitly to prevent shadow properties
        // Organization relationship (from TenantEntity)
        // Clinic relationship - explicitly use BranchId and specify the navigation property on Clinic
        // This prevents EF Core from creating shadow properties
        builder.HasOne(x => x.Branch)
            .WithMany(c => c.Invoices)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Patient relationship - explicitly use PatientId and specify the navigation property on Patient
        // This prevents EF Core from creating shadow properties
        builder.HasOne(x => x.Patient)
            .WithMany(p => p.Invoices)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasOne(x => x.Consultation)
            .WithOne(x => x.Invoice)
            .HasForeignKey<Invoice>(x => x.ConsultationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Prescription)
            .WithMany()
            .HasForeignKey(x => x.PrescriptionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.InvoiceItems)
            .WithOne(x => x.Invoice)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.InvoiceNumber)
            .IsUnique()
            .HasDatabaseName("IX_Invoices_InvoiceNumber");
    }
}

public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.ToTable("InvoiceItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ItemType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.Property(x => x.UnitPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.TotalPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Invoice)
            .WithMany(x => x.InvoiceItems)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

