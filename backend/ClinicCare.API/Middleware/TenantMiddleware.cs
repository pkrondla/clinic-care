using ClinicCare.Application.Common.Interfaces;

namespace ClinicCare.API.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;
    private readonly ITenantService _tenantService;

    public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger, ITenantService tenantService)
    {
        _next = next;
        _logger = logger;
        _tenantService = tenantService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Debug logging
            Console.WriteLine($"TenantMiddleware: ENTERED for path: {context.Request.Path}");
            _logger.LogInformation("TenantMiddleware processing path: {Path}", context.Request.Path);
            
            // Skip tenant resolution for certain paths
            if (ShouldSkipTenantResolution(context.Request.Path))
            {
                Console.WriteLine($"TenantMiddleware: SKIPPING tenant resolution for path: {context.Request.Path}");
                _logger.LogInformation("Skipping tenant resolution for path: {Path}", context.Request.Path);
                Console.WriteLine($"TenantMiddleware: About to call next() for path: {context.Request.Path}");
                try
                {
                    await _next(context);
                    Console.WriteLine($"TenantMiddleware: Completed for path: {context.Request.Path}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"TenantMiddleware: EXCEPTION when calling next() for path: {context.Request.Path}: {ex.GetType().Name} - {ex.Message}");
                    Console.WriteLine($"TenantMiddleware: Stack trace: {ex.StackTrace}");
                    throw; // Re-throw to let other middleware handle it
                }
                return;
            }

            // Resolve tenant
            var organizationId = await _tenantService.GetOrganizationIdAsync();
            
            _logger.LogInformation("Request processed for Organization ID: {OrganizationId}, Subdomain: {Subdomain}", 
                organizationId, _tenantService.Subdomain);

            await _next(context);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Tenant resolution failed: {Message}", ex.Message);
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid tenant or subdomain");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error in tenant middleware for path: {Path}. Error: {Message}", 
                context.Request.Path, ex.Message);
            
            // Log the full exception details
            _logger.LogError(ex, "Full exception details for tenant middleware");
            
            // Set a 500 status and return error details
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            var errorResponse = new { error = "Internal server error", details = ex.Message };
            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(errorResponse));
        }
    }

    private static bool ShouldSkipTenantResolution(PathString path)
    {
        var skipPaths = new[]
        {
            "/health",
            "/swagger",
            "/api/auth",  // Skip ALL auth endpoints
            "/api/global"
        };

        // Debug logging to see what path is being checked
        var pathValue = path.Value?.ToLowerInvariant();
        var shouldSkip = skipPaths.Any(skipPath => 
            pathValue?.StartsWith(skipPath.ToLowerInvariant()) == true);
        
        Console.WriteLine($"TenantMiddleware: Path: '{path.Value}' -> Lower: '{pathValue}' -> ShouldSkip: {shouldSkip}");
        
        return shouldSkip;
    }
}
