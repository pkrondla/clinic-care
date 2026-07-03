using HomoeoDesk.Tenant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomoeoDesk.Tenant.Infrastructure.Data.Configurations;

public class EmailSettingsConfiguration : IEntityTypeConfiguration<EmailSettings>
{
    public void Configure(EntityTypeBuilder<EmailSettings> builder)
    {
        builder.ToTable("EmailSettings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrganizationId)
            .IsRequired();

        builder.Property(x => x.IsEnabled)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.SmtpServer)
            .HasMaxLength(255);

        builder.Property(x => x.SmtpPort)
            .HasDefaultValue(587);

        builder.Property(x => x.UseSsl)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.UseTls)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.SmtpUsername)
            .HasMaxLength(255);

        builder.Property(x => x.SmtpPassword)
            .HasMaxLength(1000); // Encrypted

        builder.Property(x => x.FromEmail)
            .HasMaxLength(255);

        builder.Property(x => x.FromName)
            .HasMaxLength(255);

        builder.Property(x => x.ReplyToEmail)
            .HasMaxLength(255);

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
        // Unique constraint: One email setting per organization
        builder.HasIndex(x => x.OrganizationId)
            .IsUnique()
            .HasDatabaseName("IX_EmailSettings_OrganizationId");
    }
}

