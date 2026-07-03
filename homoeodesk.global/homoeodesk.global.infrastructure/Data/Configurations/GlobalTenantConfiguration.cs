using HomoeoDesk.Global.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomoeoDesk.Global.Infrastructure.Data.Configurations;

public class GlobalTenantConfiguration : IEntityTypeConfiguration<GlobalTenant>
{
    public void Configure(EntityTypeBuilder<GlobalTenant> builder)
    {
        builder.ToTable("Organizations", "dbo");

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

        builder.Property(x => x.Subdomain)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ContactEmail)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.ContactPhone)
            .HasMaxLength(20);

        builder.Property(x => x.Address)
            .HasMaxLength(500);

        builder.Property(x => x.DatabaseName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.SubscriptionStatus)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.TrialEndDate)
            .IsRequired(false);

        builder.HasIndex(x => x.Subdomain)
            .IsUnique()
            .HasDatabaseName("IX_Organizations_Subdomain");

        builder.HasIndex(x => x.DatabaseName)
            .IsUnique()
            .HasDatabaseName("IX_Organizations_DatabaseName");
    }
}
