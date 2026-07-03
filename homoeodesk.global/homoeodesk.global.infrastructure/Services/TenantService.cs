using HomoeoDesk.Global.Application.Common.Interfaces;
using HomoeoDesk.Global.Application.Common.Interfaces.Global;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Global.Infrastructure.Services;

public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IGlobalDbContext _globalContext;
    private int? _tenantId;
    private string? _subdomain;

    public TenantService(IHttpContextAccessor httpContextAccessor, IGlobalDbContext globalContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _globalContext = globalContext;
    }

    public int? TenantId => _tenantId;
    public string? Subdomain => _subdomain;

    public async Task<int> GetTenantIdAsync()
    {
        if (_tenantId.HasValue)
        {
            return _tenantId.Value;
        }

        var subdomain = GetSubdomainFromRequest();
        if (string.IsNullOrEmpty(subdomain))
        {
            subdomain = "demo";
        }

        var tenant = await _globalContext.GlobalTenants
            .FirstOrDefaultAsync(x => x.Subdomain == subdomain && x.IsActive);

        if (tenant == null)
        {
            throw new UnauthorizedAccessException($"Tenant not found for subdomain: {subdomain}");
        }

        _tenantId = tenant.Id;
        _subdomain = subdomain;

        return _tenantId.Value;
    }

    public async Task<bool> IsValidTenantAsync(string subdomain)
    {
        return await _globalContext.GlobalTenants
            .AnyAsync(x => x.Subdomain == subdomain && x.IsActive);
    }

    private string? GetSubdomainFromRequest()
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            return "demo";
        }

        var request = _httpContextAccessor.HttpContext.Request;
        var host = request.Host.Host;

        if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
            host.StartsWith("localhost:", StringComparison.OrdinalIgnoreCase))
        {
            return "demo";
        }

        var parts = host.Split('.');
        if (parts.Length >= 3)
        {
            return parts[0];
        }

        if (request.Headers.ContainsKey("X-Tenant-Subdomain"))
        {
            return request.Headers["X-Tenant-Subdomain"].FirstOrDefault();
        }

        if (request.Query.ContainsKey("tenant"))
        {
            return request.Query["tenant"].FirstOrDefault();
        }

        return null;
    }
}
