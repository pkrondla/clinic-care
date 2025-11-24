using ClinicCare.Application.Features.Inventory.Commands.PerformStockAudit;
using ClinicCare.Application.Features.Inventory.Queries.GetStockAuditHistory;
using ClinicCare.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace ClinicCare.API.Modules.Tenant;

public static class StockAuditEndpoints
{
    public static IEndpointRouteBuilder MapStockAuditEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/stock-audit")
            .WithTags("StockAudit")
            .WithOpenApi()
            .RequireAuthorization();

        // Perform stock audit
        group.MapPost("/", PerformStockAudit)
            .WithName("PerformStockAudit")
            .WithSummary("Perform a physical stock audit and adjust inventory")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Get stock audit history
        group.MapGet("/history", GetStockAuditHistory)
            .WithName("GetStockAuditHistory")
            .WithSummary("Get stock audit history")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IResult> PerformStockAudit(
        PerformStockAuditCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        if (!result.Succeeded)
        {
            return Results.BadRequest(new { message = "Failed to perform stock audit", errors = result.Errors });
        }

        return Results.Ok(new { message = "Stock audit completed successfully", data = result.Data });
    }

    private static async Task<IResult> GetStockAuditHistory(
        int? clinicId,
        DateTime? startDate,
        DateTime? endDate,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetStockAuditHistoryQuery
        {
            ClinicId = clinicId,
            StartDate = startDate,
            EndDate = endDate
        };
        var result = await mediator.Send(query, cancellationToken);

        if (!result.Succeeded)
        {
            return Results.BadRequest(new { message = "Failed to retrieve stock audit history", errors = result.Errors });
        }

        return Results.Ok(new { data = result.Data });
    }
}

