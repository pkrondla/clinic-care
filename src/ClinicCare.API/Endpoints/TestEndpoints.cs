using ClinicCare.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.API.Endpoints;

public static class TestEndpoints
{
    public static IEndpointRouteBuilder MapTestEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/test")
            .WithTags("Test")
            .WithOpenApi();

        group.MapGet("/tenant", async (ITenantService tenantService) =>
        {
            try
            {
                var organizationId = await tenantService.GetOrganizationIdAsync();
                var subdomain = tenantService.Subdomain;
                
                return Results.Ok(new
                {
                    success = true,
                    organizationId = organizationId,
                    subdomain = subdomain,
                    message = "Tenant resolution successful"
                });
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: 500,
                    title: "Tenant Resolution Failed"
                );
            }
        })
        .WithName("TestTenantResolution")
        .WithSummary("Test tenant resolution")
        .WithDescription("Tests if the tenant service can resolve the current tenant");

        return app;
    }
}
