using ClinicCare.Domain.Global;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Common.Interfaces.Global;

public interface IGlobalDbContext
{
    DbSet<Organization> Organizations { get; }
    DbSet<SubscriptionPlan> SubscriptionPlans { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}