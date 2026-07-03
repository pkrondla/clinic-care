using HomoeoDesk.Global.Application.Common.Interfaces.Global;
using HomoeoDesk.Global.Domain.Common;
using HomoeoDesk.Global.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace HomoeoDesk.Global.Infrastructure.Data;

public class GlobalDbContext : DbContext, IGlobalDbContext
{
    public GlobalDbContext(DbContextOptions<GlobalDbContext> options)
        : base(options)
    {
    }

    public DbSet<GlobalTenant> GlobalTenants => Set<GlobalTenant>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<OrganizationSubscription> OrganizationSubscriptions => Set<OrganizationSubscription>();
    public DbSet<GlobalMedicine> GlobalMedicines => Set<GlobalMedicine>();
    public DbSet<SystemUser> SystemUsers => Set<SystemUser>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureGlobalEntities(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        var tenantEntity = builder.Entity<GlobalTenant>();
        var isActiveProperty = tenantEntity.Metadata.FindProperty("IsActive");
        if (isActiveProperty == null || isActiveProperty.IsShadowProperty())
        {
            tenantEntity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true)
                .HasColumnName("IsActive");
        }
    }

    private static void ConfigureGlobalEntities(ModelBuilder builder)
    {
        builder.Entity<SubscriptionPlan>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(10,2)");
        });

        builder.Entity<OrganizationSubscription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Ignore(e => e.IsActive);

            entity.Property(e => e.TenantId).HasColumnName("OrganizationId");

            entity.HasOne(e => e.GlobalTenant)
                .WithMany(o => o.Subscriptions)
                .HasForeignKey(e => e.TenantId);

            entity.HasOne(e => e.SubscriptionPlan)
                .WithMany(p => p.OrganizationSubscriptions)
                .HasForeignKey(e => e.SubscriptionPlanId);
        });

        builder.Entity<SystemUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(500);
        });

        builder.Entity<PaymentTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TransactionId).IsUnique();
            entity.Property(e => e.Amount).HasColumnType("decimal(10,2)");
            entity.Property(e => e.TenantId).HasColumnName("OrganizationId");

            entity.HasOne(e => e.GlobalTenant)
                .WithMany(o => o.PaymentTransactions)
                .HasForeignKey(e => e.TenantId);

            entity.HasOne(e => e.Subscription)
                .WithMany(s => s.PaymentTransactions)
                .HasForeignKey(e => e.SubscriptionId);
        });

        builder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).HasColumnName("OrganizationId");
            entity.HasIndex(e => new { e.TenantId, e.Timestamp });
            entity.HasIndex(e => e.Timestamp);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
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
