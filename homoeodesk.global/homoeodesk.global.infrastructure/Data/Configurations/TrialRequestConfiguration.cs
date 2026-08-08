using HomoeoDesk.Global.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomoeoDesk.Global.Infrastructure.Data.Configurations;

public class TrialRequestConfiguration : IEntityTypeConfiguration<TrialRequest>
{
    public void Configure(EntityTypeBuilder<TrialRequest> builder)
    {
        builder.ToTable("TrialRequests");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.FullName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(255);
        builder.Property(e => e.Phone).HasMaxLength(40);
        builder.Property(e => e.ClinicName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.City).HasMaxLength(120);
        builder.Property(e => e.PatientsPerWeek).HasMaxLength(40);
        builder.Property(e => e.Message).HasMaxLength(2000);
        builder.Property(e => e.Source).IsRequired().HasMaxLength(100);
        builder.HasIndex(e => new { e.Status, e.CreatedAt });
        builder.HasIndex(e => e.Email);
    }
}
