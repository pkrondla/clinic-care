using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Interfaces.Global;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace ClinicCare.API.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ITenantService tenantService,
        IGlobalDbContext globalDbContext)
    {
        var host = context.Request.Host.Value;

        // Check if this is the global domain
        if (!host.StartsWith("www.") && !host.Contains("."))
        {
            await _next(context);
            return;
        }

        // Extract subdomain
        var subdomain = host.Split('.')[0];
        if (subdomain == "www")
        {
            await _next(context);
            return;
        }

        // Verify tenant exists and is active
        var organization = await globalDbContext.Organizations
            .FirstOrDefaultAsync(o => o.Subdomain == subdomain && o.IsActive);

        if (organization == null)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsJsonAsync(new { error = "Tenant not found" });
            return;
        }

        // Set tenant ID for the request
        tenantService.SetTenantId(organization.Id.ToString());

        await _next(context);
    }
}