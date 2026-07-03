using HomoeoDesk.Global.Application.Common.Interfaces.Global;
using HomoeoDesk.Global.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Global.Application.Common;

internal static class GlobalTenantQueries
{
    public static async Task<bool> SubdomainExistsAsync(
        IGlobalDbContext context,
        string subdomain,
        int? excludeId,
        CancellationToken cancellationToken)
    {
        var query = context.GlobalTenants.Where(o => o.Subdomain == subdomain);
        if (excludeId.HasValue)
            query = query.Where(o => o.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public static async Task<string> GenerateSubdomainAsync(
        IGlobalDbContext context,
        string tenantName,
        CancellationToken cancellationToken)
    {
        var baseSubdomain = new string(tenantName
            .ToLower()
            .Replace(" ", "")
            .Replace("&", "and")
            .Where(char.IsLetterOrDigit)
            .Take(50)
            .ToArray());

        if (string.IsNullOrWhiteSpace(baseSubdomain))
            baseSubdomain = "tenant";

        var subdomain = baseSubdomain;
        var counter = 1;

        while (await SubdomainExistsAsync(context, subdomain, null, cancellationToken))
        {
            subdomain = $"{baseSubdomain}{counter}";
            counter++;
        }

        return subdomain;
    }

    public static async Task<GlobalTenant?> GetByIdAsync(
        IGlobalDbContext context,
        int id,
        CancellationToken cancellationToken) =>
        await context.GlobalTenants
            .Include(o => o.Subscriptions)
            .FirstOrDefaultAsync(o => o.Id == id && o.IsActive, cancellationToken);

    public static async Task<GlobalTenant?> GetBySubdomainAsync(
        IGlobalDbContext context,
        string subdomain,
        CancellationToken cancellationToken) =>
        await context.GlobalTenants
            .Include(o => o.Subscriptions)
            .FirstOrDefaultAsync(o => o.Subdomain == subdomain && o.IsActive, cancellationToken);
}
