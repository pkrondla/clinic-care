using ClinicCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Common.Interfaces.Global;

/// <summary>
/// Global Database Context Interface
/// Handles system-wide data: Organizations, Subscriptions, Global Medicines, System Users
/// Database: ClinicCare_Global
/// </summary>
public interface IGlobalDbContext
{
    // Organizations & Subscriptions
    DbSet<Organization> Organizations { get; }
    DbSet<SubscriptionPlan> SubscriptionPlans { get; }
    DbSet<OrganizationSubscription> OrganizationSubscriptions { get; }
    
    // Global Medicine Database (Shared Catalog)
    DbSet<GlobalMedicine> GlobalMedicines { get; }
    
    // System Users (Super Admins)
    DbSet<SystemUser> SystemUsers { get; }
    
    // Payment Transactions
    DbSet<PaymentTransaction> PaymentTransactions { get; }
    
    // Audit Logs
    DbSet<AuditLog> AuditLogs { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

