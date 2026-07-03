using HomoeoDesk.Tenant.Application.Common.Interfaces;

namespace HomoeoDesk.Tenant.Api.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;

    public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
    {
        if (ShouldSkipTenantResolution(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var tenantId = await tenantService.GetTenantIdAsync();
        _logger.LogDebug("Request scoped to tenant {TenantId} ({Subdomain})", tenantId, tenantService.Subdomain);

        await _next(context);
    }

    private static bool ShouldSkipTenantResolution(PathString path)
    {
        var pathValue = path.Value?.ToLowerInvariant() ?? string.Empty;
        string[] skipPaths = ["/health", "/swagger", "/api/auth", "/api/global"];
        return skipPaths.Any(skipPath => pathValue.StartsWith(skipPath, StringComparison.Ordinal));
    }
}
