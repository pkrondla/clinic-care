using HomoeoDesk.Tenant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomoeoDesk.Tenant.Infrastructure.Data.Configurations;

public class NotificationPreferencesConfiguration : IEntityTypeConfiguration<NotificationPreferences>
{
    public void Configure(EntityTypeBuilder<NotificationPreferences> builder)
    {
        builder.ToTable("NotificationPreferences");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrganizationId)
            .IsRequired();

        builder.Property(x => x.NotificationType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.EnableWhatsApp)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.EnableEmail)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.EnableSMS)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.Template)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Relationships
        // Unique constraint: One preference per organization per notification type
        builder.HasIndex(x => new { x.OrganizationId, x.NotificationType })
            .IsUnique()
            .HasDatabaseName("IX_NotificationPreferences_OrganizationId_NotificationType");

        // Index for active preferences
        builder.HasIndex(x => new { x.OrganizationId, x.IsActive })
            .HasDatabaseName("IX_NotificationPreferences_OrganizationId_IsActive");
    }
}

