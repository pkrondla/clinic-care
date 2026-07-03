using HomoeoDesk.Tenant.Domain.Entities;
using HomoeoDesk.Tenant.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomoeoDesk.Tenant.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Phone)
            .HasMaxLength(20);

        builder.Property(x => x.Role)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.SelectedBranchId)
            .HasColumnName("SelectedClinicId");

        // Computed property
        builder.Ignore(x => x.FullName);

        // Unique constraints within tenant
        builder.HasIndex(x => new { x.OrganizationId, x.Email })
            .IsUnique()
            .HasDatabaseName("IX_Users_OrganizationId_Email");

        // Relationships
        builder.HasOne(x => x.DoctorProfile)
            .WithOne(x => x.User)
            .HasForeignKey<DoctorProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Patient)
            .WithOne(x => x.User)
            .HasForeignKey<Patient>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
