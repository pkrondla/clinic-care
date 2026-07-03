using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Domain.Common;
using HomoeoDesk.Tenant.Domain.Entities;
using HomoeoDesk.Tenant.Domain.Modules.Appointments.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Reflection;

namespace HomoeoDesk.Tenant.Infrastructure.Data;

/// <summary>
/// Consolidated tenant database context (formerly ApplicationDbContext + TenantDbContext).
/// Database: HomoeoDesk_{TenantId} — one database per tenant stamp.
/// </summary>
public class TenantDbContext : DbContext, IApplicationDbContext
{
    private static readonly HashSet<Type> BranchScopedTypes =
    [
        typeof(Appointment),
        typeof(PurchaseOrder),
        typeof(Invoice),
        typeof(Inventory),
        typeof(StockTransaction),
        typeof(DoctorAvailability),
        typeof(ClinicMedicine)
    ];

    private readonly ITenantService _tenantService;
    private readonly IBranchService _branchService;
    private readonly IPublisher _publisher;

    public TenantDbContext(
        DbContextOptions<TenantDbContext> options,
        ITenantService tenantService,
        IBranchService branchService,
        IPublisher publisher)
        : base(options)
    {
        _tenantService = tenantService;
        _branchService = branchService;
        _publisher = publisher;
    }

    DatabaseFacade IApplicationDbContext.Database => Database;

    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserBranchAccess> UserBranchAccess => Set<UserBranchAccess>();
    public DbSet<DoctorProfile> DoctorProfiles => Set<DoctorProfile>();
    public DbSet<DoctorAvailability> DoctorAvailabilities => Set<DoctorAvailability>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Consultation> Consultations => Set<Consultation>();
    public DbSet<ConsultationPhoto> ConsultationPhotos => Set<ConsultationPhoto>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
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
    public DbSet<WhatsAppBusinessSettings> WhatsAppBusinessSettings => Set<WhatsAppBusinessSettings>();
    public DbSet<EmailSettings> EmailSettings => Set<EmailSettings>();
    public DbSet<SmsSettings> SmsSettings => Set<SmsSettings>();
    public DbSet<NotificationPreferences> NotificationPreferences => Set<NotificationPreferences>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Ignore<DomainEvent>();
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        RemoveShadowProperties(builder);
        ApplyQueryFilters(builder);
    }

    private static void RemoveShadowProperties(ModelBuilder builder)
    {
        var shadowPropertyNames = new[]
        {
            "AppointmentId1", "AppointmentId2", "AppointmentId3", "AppointmentId4", "AppointmentId5",
            "ClinicMedicineId", "ClinicMedicineId1", "ClinicMedicineId2"
        };

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            foreach (var propertyName in shadowPropertyNames)
            {
                var prop = entityType.FindProperty(propertyName);
                if (prop == null || !prop.IsShadowProperty()) continue;

                foreach (var fk in entityType.GetForeignKeys()
                             .Where(fk => fk.Properties.Any(p => p.Name == propertyName))
                             .ToList())
                {
                    try { entityType.RemoveForeignKey(fk); } catch { /* best effort */ }
                }

                try { entityType.RemoveProperty(prop); } catch { /* best effort */ }
            }
        }
    }

    private void ApplyQueryFilters(ModelBuilder builder)
    {
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (!typeof(TenantEntity).IsAssignableFrom(entityType.ClrType)) continue;

            var method = GetType()
                .GetMethod(nameof(ConfigureEntityFilter), BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(entityType.ClrType);
            method.Invoke(this, [builder]);
        }
    }

    private void ConfigureEntityFilter<T>(ModelBuilder builder) where T : TenantEntity
    {
        if (BranchScopedTypes.Contains(typeof(T)))
        {
            builder.Entity<T>().HasQueryFilter(e =>
                e.TenantId == _tenantService.TenantId &&
                (!_branchService.BranchId.HasValue ||
                 EF.Property<int>(e, "BranchId") == _branchService.BranchId.Value));
        }
        else
        {
            builder.Entity<T>().HasQueryFilter(e => e.TenantId == _tenantService.TenantId);
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantService.TenantId;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    if (entry.Entity is TenantEntity tenantEntity && tenantEntity.TenantId == 0 && tenantId > 0)
                        tenantEntity.TenantId = tenantId;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
