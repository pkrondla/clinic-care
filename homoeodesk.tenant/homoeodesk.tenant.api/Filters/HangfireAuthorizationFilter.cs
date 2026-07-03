using Hangfire.Dashboard;

namespace HomoeoDesk.Tenant.Api.Filters;

/// <summary>
/// Authorization filter for Hangfire dashboard
/// In production, this should check user roles (e.g., Admin only)
/// </summary>
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // For development, allow all
        // In production, check if user is authenticated and has Admin role
        var httpContext = context.GetHttpContext();
        
        // TODO: Add proper authorization check in production
        // Example:
        // return httpContext.User.Identity?.IsAuthenticated == true 
        //     && httpContext.User.IsInRole("Admin");
        
        return true; // Allow all for now
    }
}

