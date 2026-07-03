namespace HomoeoDesk.Tenant.Application.Common.Interfaces;

public interface ITenantDbContextFactory
{
    IApplicationDbContext CreateDbContext();
}