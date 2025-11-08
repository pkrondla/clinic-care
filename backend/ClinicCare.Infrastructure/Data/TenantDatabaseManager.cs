using ClinicCare.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ClinicCare.Infrastructure.Data;

public class TenantDatabaseManager
{
    private readonly TenantSettings _tenantSettings;
    private readonly DbContextOptionsBuilder<ApplicationDbContext> _optionsBuilder;

    public TenantDatabaseManager(
        IOptions<TenantSettings> tenantSettings,
        DbContextOptionsBuilder<ApplicationDbContext> optionsBuilder)
    {
        _tenantSettings = tenantSettings.Value;
        _optionsBuilder = optionsBuilder;
    }

    public async Task CreateTenantDatabaseAsync(string subdomain)
    {
        var connectionString = BuildConnectionString(subdomain);
        
        // Create database
        var options = _optionsBuilder
            .UseSqlServer(connectionString)
            .Options;

        using var context = new ApplicationDbContext(options);
        await context.Database.MigrateAsync();

        // Initialize tenant-specific data
        await InitializeTenantDataAsync(context, subdomain);
    }

    private async Task InitializeTenantDataAsync(ApplicationDbContext context, string subdomain)
    {
        var tenant = _tenantSettings.Tenants.FirstOrDefault(t => t.Subdomain == subdomain);
        if (tenant == null)
            throw new InvalidOperationException($"Tenant not found: {subdomain}");

        // Create default organization record
        var organization = new Domain.Entities.Organization
        {
            Name = tenant.Name,
            Subdomain = tenant.Subdomain,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Organizations.Add(organization);
        await context.SaveChangesAsync();
    }

    private string BuildConnectionString(string subdomain)
    {
        var databaseName = _tenantSettings.DatabaseNameTemplate.Replace("{tenant}", subdomain);
        return _tenantSettings.DefaultConnectionString.Replace("{database}", databaseName);
    }
}