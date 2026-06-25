using ClinicCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicCare.Infrastructure.Data.Configurations;

public class WhatsAppBusinessSettingsConfiguration : IEntityTypeConfiguration<WhatsAppBusinessSettings>
{
    public void Configure(EntityTypeBuilder<WhatsAppBusinessSettings> builder)
    {
        builder.ToTable("WhatsAppBusinessSettings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrganizationId)
            .IsRequired();

        builder.Property(x => x.IsEnabled)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.Provider)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(1); // Meta

        builder.Property(x => x.PhoneNumberId)
            .HasMaxLength(100);

        builder.Property(x => x.BusinessAccountId)
            .HasMaxLength(100);

        builder.Property(x => x.AccessToken)
            .HasMaxLength(1000); // Will be encrypted

        builder.Property(x => x.ApiKey)
            .HasMaxLength(500); // Will be encrypted

        builder.Property(x => x.ApiSecret)
            .HasMaxLength(500); // Will be encrypted

        builder.Property(x => x.WebhookUrl)
            .HasMaxLength(500);

        builder.Property(x => x.WebhookSecret)
            .HasMaxLength(200); // Will be encrypted

        builder.Property(x => x.WebhookVerifyToken)
            .HasMaxLength(200);

        builder.Property(x => x.ApiVersion)
            .HasMaxLength(20);

        builder.Property(x => x.FromPhoneNumber)
            .HasMaxLength(20);

        // Relationships
        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Unique constraint: One settings record per organization
        builder.HasIndex(x => x.OrganizationId)
            .IsUnique()
            .HasDatabaseName("IX_WhatsAppBusinessSettings_OrganizationId");

        // Index for active settings
        builder.HasIndex(x => new { x.OrganizationId, x.IsEnabled, x.IsActive })
            .HasDatabaseName("IX_WhatsAppBusinessSettings_OrganizationId_IsEnabled_IsActive");
    }
}

