using HomoeoDesk.Tenant.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace HomoeoDesk.Tenant.Api.Middleware;

public class TrialExpiredMiddleware
{
    private readonly RequestDelegate _next;

    public TrialExpiredMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IOptions<TenantStampOptions> stampOptions)
    {
        var options = stampOptions.Value;

        if (!options.EnforceTrialExpiry
            || options.TrialEndDate == null
            || DateTime.UtcNow <= options.TrialEndDate.Value)
        {
            await _next(context);
            return;
        }

        if (ShouldSkip(context.Request.Path))
        {
            await _next(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status402PaymentRequired;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            success = false,
            code = "TrialExpired",
            message = "Your trial period has expired. Please upgrade your subscription."
        });
    }

    private static bool ShouldSkip(PathString path)
    {
        var value = path.Value?.ToLowerInvariant() ?? string.Empty;
        string[] skip =
        [
            "/health",
            "/swagger",
            "/api/auth",
            "/hangfire",
            "/queuehub"
        ];

        return skip.Any(s => value.StartsWith(s, StringComparison.Ordinal));
    }
}
