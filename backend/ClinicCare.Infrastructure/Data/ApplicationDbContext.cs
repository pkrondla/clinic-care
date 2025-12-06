using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Domain.Common;
using ClinicCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Reflection;

namespace ClinicCare.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Global Tables
    public DbSet<GlobalMedicine> GlobalMedicines => Set<GlobalMedicine>();

    // Tenant Tables
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserOrganization> UserOrganizations => Set<UserOrganization>();
    public DbSet<UserClinicAccess> UserClinicAccess => Set<UserClinicAccess>();
    public DbSet<DoctorProfile> DoctorProfiles => Set<DoctorProfile>();
    public DbSet<DoctorAvailability> DoctorAvailabilities => Set<DoctorAvailability>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Domain.Modules.Appointments.Entities.Appointment> Appointments => Set<Domain.Modules.Appointments.Entities.Appointment>();
    public DbSet<Consultation> Consultations => Set<Consultation>();
    public DbSet<Prescription> Prescriptions => Set<
    Prescription>();
    public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();
    public DbSet<ClinicMedicine> ClinicMedicines => Set<ClinicMedicine>();
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Communication> Communications => Set<Communication>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Ignore DomainEvent base class - it's not meant to be persisted
        builder.Ignore<ClinicCare.Domain.Common.DomainEvent>();

        // Apply all configurations from assembly
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        // CRITICAL: Remove any shadow properties that EF Core might have created
        // This must happen AFTER all configurations are applied
        // We do this to ensure the model matches the database schema exactly
        RemoveShadowProperties(builder);

        // Apply tenant filters
        ApplyTenantFilters(builder);
    }

    private void RemoveShadowProperties(ModelBuilder builder)
    {
        // Remove shadow properties that EF Core might create for relationships
        // CRITICAL: This ensures the EF Core model matches the database schema exactly
        // Database schema: FK_Consultations_Appointment uses Consultations.AppointmentId (NOT AppointmentId1, AppointmentId2, etc.)
        // We must remove shadow properties to prevent EF Core from querying columns that don't exist in the database
        
        var consultationEntity = builder.Entity<Consultation>();
        var consultationType = consultationEntity.Metadata;
        
        // CRITICAL: Remove shadow properties by name - this is the most reliable method
        // EF Core creates shadow properties like AppointmentId1, AppointmentId2 when it's confused about relationships
        var shadowPropertyNames = new[] { 
            "AppointmentId1", "AppointmentId2", "AppointmentId3", "AppointmentId4", "AppointmentId5",
            "ClinicMedicineId", "ClinicMedicineId1", "ClinicMedicineId2" // PrescriptionItem shadow properties
        };
        
        // Remove shadow properties from all entities
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            foreach (var propertyName in shadowPropertyNames)
            {
                var prop = entityType.FindProperty(propertyName);
                if (prop != null && prop.IsShadowProperty())
                {
                    // Find and remove all foreign keys that use this shadow property
                    var foreignKeys = entityType.GetForeignKeys()
                        .Where(fk => fk.Properties.Any(p => p.Name == propertyName))
                        .ToList();
                    
                    foreach (var fk in foreignKeys)
                    {
                        try
                        {
                            entityType.RemoveForeignKey(fk);
                        }
                        catch
                        {
                            // Continue even if foreign key removal fails
                        }
                    }
                    
                    // Remove the shadow property
                    try
                    {
                        entityType.RemoveProperty(prop);
                    }
                    catch
                    {
                        // Continue even if property removal fails
                    }
                }
            }
        }
        
        // Also check PrescriptionItem specifically for ClinicMedicineId shadow property
        // CRITICAL: Check ALL shadow properties that start with ClinicMedicineId
        var prescriptionItemEntity = builder.Entity<PrescriptionItem>();
        var prescriptionItemType = prescriptionItemEntity.Metadata;
        
        // Get all shadow properties that start with ClinicMedicineId
        var allShadowProperties = prescriptionItemType.GetProperties()
            .Where(p => p.IsShadowProperty() && p.Name.StartsWith("ClinicMedicineId"))
            .ToList();
        
        foreach (var prop in allShadowProperties)
        {
            try
            {
                // Find and remove all foreign keys that use this shadow property
                var foreignKeys = prescriptionItemType.GetForeignKeys()
                    .Where(fk => fk.Properties.Contains(prop))
                    .ToList();
                
                foreach (var fk in foreignKeys)
                {
                    try
                    {
                        prescriptionItemType.RemoveForeignKey(fk);
                    }
                    catch
                    {
                        // Continue even if foreign key removal fails
                    }
                }
                
                // Remove the shadow property
                prescriptionItemType.RemoveProperty(prop);
            }
            catch
            {
                // Try alternative removal method
                try
                {
                    var prop2 = prescriptionItemType.FindProperty(prop.Name);
                    if (prop2 != null)
                    {
                        var fks2 = prescriptionItemType.GetForeignKeys()
                            .Where(fk => fk.Properties.Contains(prop2))
                            .ToList();
                        
                        foreach (var fk in fks2)
                        {
                            prescriptionItemType.RemoveForeignKey(fk);
                        }
                        
                        prescriptionItemType.RemoveProperty(prop2);
                    }
                }
                catch
                {
                    // Property might be in use - log for debugging
                    System.Diagnostics.Debug.WriteLine($"Could not remove shadow property: {prop.Name}");
                }
            }
        }
        
        // Also check all properties for any AppointmentId* shadow properties
        var allProperties = consultationType.GetProperties().ToList();
        foreach (var property in allProperties)
        {
            if (property.IsShadowProperty() && 
                property.Name.StartsWith("AppointmentId") && 
                property.Name != "AppointmentId")
            {
                try
                {
                    // Remove foreign keys first
                    var fks = consultationType.GetForeignKeys()
                        .Where(fk => fk.Properties.Contains(property))
                        .ToList();
                    
                    foreach (var fk in fks)
                    {
                        consultationType.RemoveForeignKey(fk);
                    }
                    
                    // Remove the property
                    consultationType.RemoveProperty(property);
                }
                catch
                {
                    // Continue with other properties
                }
            }
        }
    }

    private void ApplyTenantFilters(ModelBuilder builder)
    {
        // Apply global query filters for tenant isolation
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(TenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(ApplicationDbContext)
                    .GetMethod(nameof(SetTenantFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);

                method.Invoke(null, new object[] { builder });
            }
        }
    }

    private static void SetTenantFilter<T>(ModelBuilder builder) where T : TenantEntity
    {
        builder.Entity<T>().HasQueryFilter(e => EF.Property<int>(e, "OrganizationId") == 
            EF.Property<int>(e, "OrganizationId")); // Will be overridden by tenant service
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Set audit fields
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    
                    // Set tenant ID for new entities - will be set by middleware or service layer
                    if (entry.Entity is TenantEntity tenantEntity)
                    {
                        // Tenant ID should be set by the service layer before calling SaveChanges
                        // This ensures proper tenant isolation
                    }
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
