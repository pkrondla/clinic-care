using ClinicCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicCare.Infrastructure.Data.Configurations;

public class ClinicMedicineConfiguration : IEntityTypeConfiguration<ClinicMedicine>
{
    public void Configure(EntityTypeBuilder<ClinicMedicine> builder)
    {
        builder.ToTable("ClinicMedicines");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.GenericName)
            .HasMaxLength(200);

        builder.Property(x => x.Type)
            .HasMaxLength(50);

        builder.Property(x => x.Potency)
            .HasMaxLength(50);

        builder.Property(x => x.Manufacturer)
            .HasMaxLength(200);

        builder.Property(x => x.PurchasePrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.SellingPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        // Explicitly map foreign key properties BEFORE relationships to avoid shadow properties
        builder.Property(x => x.ClinicId)
            .IsRequired();

        builder.Property(x => x.GlobalMedicineId)
            .IsRequired(false);

        // Relationships
        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Clinic)
            .WithMany()
            .HasForeignKey(x => x.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.GlobalMedicine)
            .WithMany(x => x.ClinicMedicines)
            .HasForeignKey(x => x.GlobalMedicineId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(x => new { x.OrganizationId, x.Name, x.Potency, x.Manufacturer })
            .IsUnique()
            .HasDatabaseName("IX_ClinicMedicines_NamePotencyManufacturer");
    }
}

