using HomoeoDesk.Tenant.Domain.Modules.Appointments.Entities;
using HomoeoDesk.Tenant.Domain.Modules.Appointments.ValueObjects;
using HomoeoDesk.Tenant.Domain.Enums;
using HomoeoDesk.Tenant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomoeoDesk.Tenant.Infrastructure.Data.Configurations
{
    public class AppointmentConfiguration : IEntityTypeConfiguration<Domain.Modules.Appointments.Entities.Appointment>
    {
        public void Configure(EntityTypeBuilder<Domain.Modules.Appointments.Entities.Appointment> builder)
        {
            builder.ToTable("Appointments");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.BranchId)
                .IsRequired();

            builder.Property(x => x.DoctorId)
                .IsRequired();

            builder.Property(x => x.PatientId)
                .IsRequired();

            // Configure value objects
            builder.OwnsOne(x => x.AppointmentDate, ad =>
            {
                ad.Property(x => x.Value)
                    .HasColumnName("AppointmentDate")
                    .IsRequired();
            });

            builder.Property(x => x.TokenNumber)
                .IsRequired();

            builder.Property(x => x.Type)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.Notes)
                .HasMaxLength(1000);

            // Configure relationships

            builder.HasOne(x => x.Branch)
                .WithMany(c => c.Appointments)
                .HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(x => x.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(x => x.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // Configure one-to-one relationship with Consultation
            // CRITICAL: The relationship is configured ONLY in ConsultationConfiguration
            // The foreign key (AppointmentId) is on the Consultation side
            // Appointment does NOT have a foreign key - only a navigation property
            // We do NOT configure it here to avoid duplicate configuration
            // This prevents EF Core from creating shadow properties

            // Configure indexes - Note: EF Core doesn't support indexes on value object properties directly
            // We'll create indexes on the underlying properties instead
            builder.HasIndex(x => new { x.OrganizationId, x.BranchId, x.DoctorId, x.TokenNumber })
                .HasDatabaseName("IX_Appointments_ClinicDoctorToken");

            builder.HasIndex(x => new { x.OrganizationId, x.DoctorId })
                .HasDatabaseName("IX_Appointments_Doctor");

            builder.HasIndex(x => new { x.OrganizationId, x.PatientId })
                .HasDatabaseName("IX_Appointments_Patient");
        }
    }
}