using ClinicCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicCare.Infrastructure.Data.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.ContactPerson)
            .HasMaxLength(100);

        builder.Property(x => x.Email)
            .HasMaxLength(255);

        builder.Property(x => x.Phone)
            .HasMaxLength(20);

        builder.Property(x => x.AlternatePhone)
            .HasMaxLength(20);

        builder.Property(x => x.Address)
            .HasMaxLength(500);

        builder.Property(x => x.City)
            .HasMaxLength(100);

        builder.Property(x => x.State)
            .HasMaxLength(100);

        builder.Property(x => x.PinCode)
            .HasMaxLength(10);

        builder.Property(x => x.GSTNumber)
            .HasMaxLength(15);

        builder.Property(x => x.PANNumber)
            .HasMaxLength(10);

        builder.Property(x => x.BankName)
            .HasMaxLength(200);

        builder.Property(x => x.BankAccountNumber)
            .HasMaxLength(50);

        builder.Property(x => x.IFSCCode)
            .HasMaxLength(11);

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => new { x.OrganizationId, x.Name })
            .HasDatabaseName("IX_Suppliers_OrganizationName");
    }
}

