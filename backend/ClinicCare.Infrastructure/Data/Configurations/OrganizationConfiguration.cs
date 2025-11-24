using ClinicCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicCare.Infrastructure.Data.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations", "dbo"); // Explicitly set table and schema
        
        builder.HasKey(x => x.Id);

        // CRITICAL: Map BaseEntity properties FIRST, before any other properties or relationships
        // This ensures EF Core includes them in the model
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

        // Now map other properties
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
            .HasConversion<int>(); // Convert enum to int

        builder.Property(x => x.TrialEndDate)
            .IsRequired(false);

        // Unique constraints
        builder.HasIndex(x => x.Subdomain)
            .IsUnique()
            .HasDatabaseName("IX_Organizations_Subdomain");
        
        builder.HasIndex(x => x.DatabaseName)
            .IsUnique()
            .HasDatabaseName("IX_Organizations_DatabaseName");

        // Relationships
        builder.HasMany(x => x.Clinics)
            .WithOne(x => x.Organization)
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Users)
            .WithOne(x => x.Organization)
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
