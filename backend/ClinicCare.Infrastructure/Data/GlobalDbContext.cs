using ClinicCare.Application.Common.Interfaces.Global;
using ClinicCare.Domain.Common;
using ClinicCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace ClinicCare.Infrastructure.Data;

/// <summary>
/// Global Database Context - handles system-wide data
/// Database: ClinicCare_Global
/// </summary>
public class GlobalDbContext : DbContext, IGlobalDbContext
{
    public GlobalDbContext(DbContextOptions<GlobalDbContext> options)
        : base(options)
    {
    }

    // Organizations & Subscriptions
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<OrganizationSubscription> OrganizationSubscriptions => Set<OrganizationSubscription>();
    
    // Global Medicine Database
    public DbSet<GlobalMedicine> GlobalMedicines => Set<GlobalMedicine>();
    
    // System Users
    public DbSet<SystemUser> SystemUsers => Set<SystemUser>();
    
    // Payment Transactions
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    
    // Audit Logs
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Ignore DomainEvent base class
        builder.Ignore<DomainEvent>();

        // Configure entity relationships FIRST (before applying configurations)
        // This ensures base configuration is set up
        ConfigureGlobalEntities(builder);
        
        // Apply all configurations from assembly (this includes OrganizationConfiguration)
        // This will override any base configuration with explicit mappings
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        // FORCE Organization.IsActive to be mapped AFTER all configurations
        // This is a workaround to ensure the property is definitely included
        var orgEntity = builder.Entity<Organization>();
        var isActiveProperty = orgEntity.Metadata.FindProperty("IsActive");
        if (isActiveProperty == null || isActiveProperty.IsShadowProperty())
        {
            // Property not mapped or is shadow property - force map it
            orgEntity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true)
                .HasColumnName("IsActive");
        }
    }

    private void ConfigureGlobalEntities(ModelBuilder builder)
    {
        // Organization configuration is fully handled by OrganizationConfiguration class
        // No need to configure it here to avoid conflicts

        // SubscriptionPlan
        builder.Entity<SubscriptionPlan>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(10,2)");
        });

        // OrganizationSubscription
        builder.Entity<OrganizationSubscription>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // OrganizationSubscriptions table does NOT have IsActive column
            // Ignore it since subscriptions use Status field instead
            entity.Ignore(e => e.IsActive);
            
            entity.HasOne(e => e.Organization)
                .WithMany(o => o.Subscriptions)
                .HasForeignKey(e => e.OrganizationId);
            entity.HasOne(e => e.SubscriptionPlan)
                .WithMany(p => p.OrganizationSubscriptions)
                .HasForeignKey(e => e.SubscriptionPlanId);
        });

        // GlobalMedicine configuration is handled by GlobalMedicineConfiguration class
        // No need to configure it here to avoid conflicts

        // SystemUser
        builder.Entity<SystemUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(500);
        });

        // PaymentTransaction
        builder.Entity<PaymentTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TransactionId).IsUnique();
            entity.Property(e => e.Amount).HasColumnType("decimal(10,2)");
            entity.HasOne(e => e.Organization)
                .WithMany(o => o.PaymentTransactions)
                .HasForeignKey(e => e.OrganizationId);
            entity.HasOne(e => e.Subscription)
                .WithMany(s => s.PaymentTransactions)
                .HasForeignKey(e => e.SubscriptionId);
        });

        // AuditLog
        builder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.OrganizationId, e.Timestamp });
            entity.HasIndex(e => e.Timestamp);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
        });
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

