using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Interfaces.Global;
using ClinicCare.Domain.Global;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ClinicCare.Infrastructure.Data.Global;

public class GlobalDbContext : DbContext, IGlobalDbContext
{
    private readonly IConfiguration _configuration;
    private readonly ICurrentUserService _currentUserService;

    public GlobalDbContext(
        DbContextOptions<GlobalDbContext> options,
        IConfiguration configuration,
        ICurrentUserService currentUserService) 
        : base(options)
    {
        _configuration = configuration;
        _currentUserService = currentUserService;
    }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GlobalDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = _currentUserService.UserId;
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedBy = _currentUserService.UserId;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}