using HomoeoDesk.Tenant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomoeoDesk.Tenant.Infrastructure.Data.Configurations;

public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> builder)
    {
        builder.ToTable("Inventories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CurrentStock)
            .IsRequired();

        builder.Property(x => x.MinimumStock)
            .IsRequired();

        builder.Property(x => x.MaximumStock)
            .IsRequired();

        builder.Property(x => x.PurchasePrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.SellingPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.ExpiryDate)
            .IsRequired()
            .HasConversion(
                v => v.ToDateTime(TimeOnly.MinValue),
                v => DateOnly.FromDateTime(v));

        builder.Property(x => x.BatchNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.LastUpdated)
            .IsRequired();

        // Explicitly map foreign key properties BEFORE relationships to avoid shadow properties
        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.Property(x => x.MedicineId)
            .IsRequired();

        // Relationships
        // Clinic relationship - explicitly use BranchId and specify the navigation property on Clinic
        // This prevents EF Core from creating shadow properties
        builder.HasOne(x => x.Branch)
            .WithMany(c => c.Inventories)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasOne(x => x.Medicine)
            .WithMany(x => x.Inventories)
            .HasForeignKey(x => x.MedicineId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => new { x.OrganizationId, x.BranchId, x.MedicineId })
            .HasDatabaseName("IX_Inventories_ClinicMedicine");
    }
}

public class StockTransactionConfiguration : IEntityTypeConfiguration<StockTransaction>
{
    public void Configure(EntityTypeBuilder<StockTransaction> builder)
    {
        builder.ToTable("StockTransactions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TransactionType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.Property(x => x.UnitPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.Reference)
            .HasMaxLength(100);

        builder.Property(x => x.Notes)
            .HasMaxLength(500);

        builder.Property(x => x.TransactionDate)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Medicine)
            .WithMany(x => x.StockTransactions)
            .HasForeignKey(x => x.MedicineId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.FromBranch)
            .WithMany()
            .HasForeignKey(x => x.FromBranchId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.ToBranch)
            .WithMany()
            .HasForeignKey(x => x.ToBranchId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(x => new { x.OrganizationId, x.BranchId, x.TransactionDate })
            .HasDatabaseName("IX_StockTransactions_ClinicDate");
    }
}

