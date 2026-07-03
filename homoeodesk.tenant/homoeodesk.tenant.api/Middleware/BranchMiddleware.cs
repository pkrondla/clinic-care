using HomoeoDesk.Tenant.Application.Common.Interfaces;

namespace HomoeoDesk.Tenant.Api.Middleware;

public class BranchMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<BranchMiddleware> _logger;

    public BranchMiddleware(RequestDelegate next, ILogger<BranchMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IBranchService branchService)
    {
        if (branchService.BranchId.HasValue)
        {
            _logger.LogDebug("Request scoped to branch {BranchId}", branchService.BranchId.Value);
        }

        await _next(context);
    }
}
