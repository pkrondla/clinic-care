using HomoeoDesk.Global.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Global.Application.Common.Interfaces.Global;

public interface IGlobalDbContext
{
    DbSet<GlobalTenant> GlobalTenants { get; }
    DbSet<SubscriptionPlan> SubscriptionPlans { get; }
    DbSet<OrganizationSubscription> OrganizationSubscriptions { get; }
    DbSet<GlobalMedicine> GlobalMedicines { get; }
    DbSet<SystemUser> SystemUsers { get; }
    DbSet<PaymentTransaction> PaymentTransactions { get; }
    DbSet<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
