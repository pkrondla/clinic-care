using Microsoft.EntityFrameworkCore;
using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace ClinicCare.Infrastructure.Data;

public class TenantDbContextFactory : ITenantDbContextFactory
{
    private readonly DbContextOptionsBuilder<ApplicationDbContext> _optionsBuilder;
    private readonly TenantSettings _tenantSettings;
    private readonly ITenantService _tenantService;
    
    public TenantDbContextFactory(
        DbContextOptionsBuilder<ApplicationDbContext> optionsBuilder,
        IOptions<TenantSettings> tenantSettings,
        ITenantService tenantService)
    {
        _optionsBuilder = optionsBuilder;
        _tenantSettings = tenantSettings.Value;
        _tenantService = tenantService;
    }

    public IApplicationDbContext CreateDbContext()
    {
        var tenant = _tenantSettings.Tenants
            .FirstOrDefault(t => t.Subdomain == _tenantService.Subdomain);

        if (tenant == null)
        {
            throw new InvalidOperationException($"Tenant not found for subdomain: {_tenantService.Subdomain}");
        }

        // Use tenant-specific connection string if provided, otherwise build from template
        var connectionString = tenant.ConnectionString ?? BuildConnectionString(tenant.Subdomain);
        
        var options = _optionsBuilder
            .UseSqlServer(connectionString)
            .Options;

        return new ApplicationDbContext(options);
    }

    private string BuildConnectionString(string subdomain)
    {
        // Replace template with actual database name
        var databaseName = _tenantSettings.DatabaseNameTemplate.Replace("{tenant}", subdomain);
        return _tenantSettings.DefaultConnectionString.Replace("{database}", databaseName);
    }
}