using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Infrastructure.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace HomoeoDesk.Tenant.Infrastructure.Services;

public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly TenantStampOptions _tenantStampOptions;
    private int? _resolvedTenantId;
    private string? _subdomain;

    public TenantService(
        IHttpContextAccessor httpContextAccessor,
        IOptions<TenantStampOptions> tenantStampOptions)
    {
        _httpContextAccessor = httpContextAccessor;
        _tenantStampOptions = tenantStampOptions.Value;
    }

    public int TenantId => ResolveTenantId();

    public int? OrganizationId => _resolvedTenantId ?? (ResolveTenantId() == 0 ? null : ResolveTenantId());

    public string? Subdomain => _subdomain ?? ResolveSubdomain();

    public Task<int> GetTenantIdAsync() => Task.FromResult(ResolveTenantId());

    public Task<int> GetOrganizationIdAsync() => GetTenantIdAsync();

    private int ResolveTenantId()
    {
        if (_resolvedTenantId.HasValue)
            return _resolvedTenantId.Value;

        if (_tenantStampOptions.EnableFixedTenant)
        {
            _resolvedTenantId = _tenantStampOptions.FixedTenantId;
            _subdomain = _tenantStampOptions.FixedTenantSubdomain ?? "demo";
            return _resolvedTenantId.Value;
        }

        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            _resolvedTenantId = 1;
            _subdomain = "demo";
            return _resolvedTenantId.Value;
        }

        var tenantClaim = context.User.FindFirst("TenantId")
            ?? context.User.FindFirst("OrganizationId");
        if (tenantClaim != null && int.TryParse(tenantClaim.Value, out var claimTenantId))
        {
            _resolvedTenantId = claimTenantId;
            _subdomain = ResolveSubdomain();
            return claimTenantId;
        }

        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var headerValue)
            && int.TryParse(headerValue.FirstOrDefault(), out var headerTenantId))
        {
            _resolvedTenantId = headerTenantId;
            _subdomain = ResolveSubdomain();
            return headerTenantId;
        }

        _resolvedTenantId = 1;
        _subdomain = ResolveSubdomain();
        return _resolvedTenantId.Value;
    }

    private string? ResolveSubdomain()
    {
        if (_tenantStampOptions.EnableFixedTenant)
            return _tenantStampOptions.FixedTenantSubdomain ?? "demo";

        var context = _httpContextAccessor.HttpContext;
        if (context == null)
            return "demo";

        var host = context.Request.Host.Host;
        if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
            || host.StartsWith("localhost:", StringComparison.OrdinalIgnoreCase))
            return "demo";

        var parts = host.Split('.');
        if (parts.Length >= 3)
            return parts[0];

        if (context.Request.Headers.TryGetValue("X-Tenant-Subdomain", out var subdomainHeader))
            return subdomainHeader.FirstOrDefault();

        if (context.Request.Query.TryGetValue("tenant", out var tenantQuery))
            return tenantQuery.FirstOrDefault();

        return "demo";
    }
}
