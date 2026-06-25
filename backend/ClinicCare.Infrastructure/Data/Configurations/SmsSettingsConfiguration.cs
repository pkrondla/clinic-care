using ClinicCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicCare.Infrastructure.Data.Configurations;

public class SmsSettingsConfiguration : IEntityTypeConfiguration<SmsSettings>
{
    public void Configure(EntityTypeBuilder<SmsSettings> builder)
    {
        builder.ToTable("SmsSettings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrganizationId)
            .IsRequired();

        builder.Property(x => x.IsEnabled)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.Provider)
            .HasMaxLength(50);

        builder.Property(x => x.ApiKey)
            .HasMaxLength(500); // Encrypted

        builder.Property(x => x.ApiSecret)
            .HasMaxLength(500); // Encrypted

        builder.Property(x => x.AccountSid)
            .HasMaxLength(100);

        builder.Property(x => x.AuthToken)
            .HasMaxLength(500); // Encrypted

        builder.Property(x => x.FromPhoneNumber)
            .HasMaxLength(50);

        builder.Property(x => x.SenderId)
            .HasMaxLength(50);

        builder.Property(x => x.ApiUrl)
            .HasMaxLength(500);

        builder.Property(x => x.TimeoutSeconds)
            .HasDefaultValue(30);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Unique constraint: One SMS setting per organization
        builder.HasIndex(x => x.OrganizationId)
            .IsUnique()
            .HasDatabaseName("IX_SmsSettings_OrganizationId");
    }
}

