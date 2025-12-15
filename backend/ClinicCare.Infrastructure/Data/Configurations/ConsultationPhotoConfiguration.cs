using ClinicCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicCare.Infrastructure.Data.Configurations;

public class ConsultationPhotoConfiguration : IEntityTypeConfiguration<ConsultationPhoto>
{
    public void Configure(EntityTypeBuilder<ConsultationPhoto> builder)
    {
        builder.ToTable("ConsultationPhotos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ConsultationId)
            .IsRequired();

        builder.Property(x => x.PhotoUrl)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        // Relationships
        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Consultation)
            .WithMany(c => c.Photos)
            .HasForeignKey(x => x.ConsultationId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        // Indexes
        builder.HasIndex(x => x.ConsultationId)
            .HasDatabaseName("IX_ConsultationPhotos_ConsultationId");
    }
}

