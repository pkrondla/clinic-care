using HomoeoDesk.Tenant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomoeoDesk.Tenant.Infrastructure.Data.Configurations;

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

        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.Property(x => x.GlobalMedicineId)
            .IsRequired(false);

        builder.HasOne(x => x.Branch)
            .WithMany(c => c.ClinicMedicines)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasIndex(x => new { x.OrganizationId, x.Name, x.Potency, x.Manufacturer })
            .IsUnique()
            .HasDatabaseName("IX_ClinicMedicines_NamePotencyManufacturer");
    }
}
