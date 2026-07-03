using HomoeoDesk.Tenant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomoeoDesk.Tenant.Infrastructure.Data.Configurations;

public class DoctorProfileConfiguration : IEntityTypeConfiguration<DoctorProfile>
{
    public void Configure(EntityTypeBuilder<DoctorProfile> builder)
    {
        builder.ToTable("DoctorProfiles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RegistrationNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Qualification)
            .HasMaxLength(200);

        builder.Property(x => x.ExperienceYears)
            .IsRequired();

        builder.Property(x => x.Specialization)
            .HasMaxLength(200);

        builder.Property(x => x.ConsultationFeeInPerson)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.ConsultationFeeTele)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.FollowupFeeInPerson)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.FollowupFeeTele)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.User)
            .WithOne(x => x.DoctorProfile)
            .HasForeignKey<DoctorProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.RegistrationNumber)
            .IsUnique()
            .HasDatabaseName("IX_DoctorProfiles_RegistrationNumber");
    }
}

