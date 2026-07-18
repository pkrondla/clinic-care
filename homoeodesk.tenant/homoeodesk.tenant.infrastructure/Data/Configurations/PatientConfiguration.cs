using HomoeoDesk.Tenant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomoeoDesk.Tenant.Infrastructure.Data.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PatientCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.FirstName)
            .HasMaxLength(100);

        builder.Property(x => x.LastName)
            .HasMaxLength(100);

        builder.Property(x => x.Phone)
            .HasMaxLength(20);

        builder.Property(x => x.Gender)
            .HasMaxLength(20);

        builder.Property(x => x.BloodGroup)
            .HasMaxLength(10);

        builder.Property(x => x.Address)
            .HasMaxLength(500);

        builder.Property(x => x.EmergencyContact)
            .HasMaxLength(500);

        builder.Ignore(x => x.Age);

        builder.HasIndex(x => x.PatientCode)
            .IsUnique()
            .HasDatabaseName("UK_Patients_PatientCode");
    }
}
