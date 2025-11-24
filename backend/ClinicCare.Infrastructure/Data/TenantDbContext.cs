using ClinicCare.Application.Common.Interfaces.Tenant;
using ClinicCare.Domain.Common;
using ClinicCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace ClinicCare.Infrastructure.Data;

/// <summary>
/// Tenant Database Context - handles organization-specific data
/// Database: ClinicCare_{TenantId}
/// Each tenant has a separate database instance
/// </summary>
public class TenantDbContext : DbContext, ITenantDbContext
{
    public TenantDbContext(DbContextOptions<TenantDbContext> options)
        : base(options)
    {
    }

    // Clinics & Users
    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserClinicAccess> UserClinicAccess => Set<UserClinicAccess>();
    public DbSet<DoctorProfile> DoctorProfiles => Set<DoctorProfile>();
    public DbSet<DoctorAvailability> DoctorAvailabilities => Set<DoctorAvailability>();
    
    // Patients
    public DbSet<Patient> Patients => Set<Patient>();
    
    // Appointments & Consultations
    public DbSet<Domain.Modules.Appointments.Entities.Appointment> Appointments => Set<Domain.Modules.Appointments.Entities.Appointment>();
    public DbSet<Consultation> Consultations => Set<Consultation>();
    
    // Prescriptions
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();
    
    // Medicines
    public DbSet<ClinicMedicine> ClinicMedicines => Set<ClinicMedicine>();
    
    // Inventory
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();
    
    // Billing
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    
    // Communications
    public DbSet<Communication> Communications => Set<Communication>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Ignore DomainEvent base class
        builder.Ignore<DomainEvent>();

        // Apply all configurations from assembly
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Configure tenant entities
        ConfigureTenantEntities(builder);
    }

    private void ConfigureTenantEntities(ModelBuilder builder)
    {
        // Clinic
        builder.Entity<Clinic>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
        });

        // User
        builder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(500);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        });

        // UserClinicAccess
        builder.Entity<UserClinicAccess>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.ClinicId }).IsUnique();
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId);
            entity.HasOne(e => e.Clinic)
                .WithMany()
                .HasForeignKey(e => e.ClinicId);
        });

        // DoctorProfile configuration is handled by DoctorProfileConfiguration class
        // No need to configure it here to avoid conflicts

        // Patient
        builder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PatientCode).IsUnique();
            entity.Property(e => e.PatientCode).IsRequired().HasMaxLength(50);
        });

        // Appointment configuration is handled by AppointmentConfiguration class
        // No need to configure it here to avoid conflicts with value object (AppointmentDate)

        // Consultation
        builder.Entity<Consultation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AppointmentId).IsUnique();
        });

        // Prescription
        builder.Entity<Prescription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PrescriptionNumber).IsUnique();
            entity.HasIndex(e => e.ConsultationId).IsUnique();
            entity.Property(e => e.PrescriptionNumber).IsRequired().HasMaxLength(50);
        });

        // ClinicMedicine, Inventory, Invoice, Prescription configurations are handled by their respective configuration classes
        // No need to configure them here to avoid conflicts
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Set audit fields for BaseEntity
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}

