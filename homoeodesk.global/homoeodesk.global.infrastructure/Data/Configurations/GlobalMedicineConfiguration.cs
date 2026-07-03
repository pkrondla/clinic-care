using HomoeoDesk.Global.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomoeoDesk.Global.Infrastructure.Data.Configurations;

public class GlobalMedicineConfiguration : IEntityTypeConfiguration<GlobalMedicine>
{
    public void Configure(EntityTypeBuilder<GlobalMedicine> builder)
    {
        builder.ToTable("GlobalMedicines", "dbo");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true)
            .HasColumnName("IsActive");

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasColumnName("CreatedAt");

        builder.Property(x => x.UpdatedAt)
            .IsRequired()
            .HasColumnName("UpdatedAt");

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.GenericName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Type)
            .HasMaxLength(50);

        builder.Property(x => x.Potency)
            .HasMaxLength(50);

        builder.Property(x => x.Manufacturer)
            .HasMaxLength(200);

        builder.Property(x => x.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired()
            .HasColumnName("MRP");

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.HasIndex(x => new { x.Name, x.Potency, x.Manufacturer })
            .IsUnique()
            .HasDatabaseName("IX_GlobalMedicines_NamePotencyManufacturer");
    }
}
