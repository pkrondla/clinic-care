using MediatR;
using Microsoft.AspNetCore.Authorization;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Reports.Queries.GetCollectionReport;
using ClinicCare.Application.Features.Reports.Queries.GetPatientReport;
using ClinicCare.Application.Features.Reports.Queries.GetInventoryReport;

namespace ClinicCare.API.Modules.Reports;

public static class ReportsEndpoints
{
    public static IEndpointRouteBuilder MapReportsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reports")
            .WithTags("Reports")
            .WithOpenApi()
            .RequireAuthorization();

        // Collection Report
        group.MapGet("/collection", GetCollectionReport)
            .WithName("GetCollectionReport")
            .WithSummary("Get collection report with filtering options")
            .Produces<Result<CollectionReportDto>>(StatusCodes.Status200OK)
            .Produces<Result<CollectionReportDto>>(StatusCodes.Status400BadRequest);

        // Patient Report
        group.MapGet("/patient", GetPatientReport)
            .WithName("GetPatientReport")
            .WithSummary("Get comprehensive patient report")
            .Produces<Result<PatientReportDto>>(StatusCodes.Status200OK)
            .Produces<Result<PatientReportDto>>(StatusCodes.Status400BadRequest);

        // Inventory Report
        group.MapGet("/inventory", GetInventoryReport)
            .WithName("GetInventoryReport")
            .WithSummary("Get combined organization inventory report")
            .Produces<Result<InventoryReportDto>>(StatusCodes.Status200OK)
            .Produces<Result<InventoryReportDto>>(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IResult> GetCollectionReport(
        [AsParameters] GetCollectionReportQuery query,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken);
        return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetPatientReport(
        [AsParameters] GetPatientReportQuery query,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken);
        return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetInventoryReport(
        [AsParameters] GetInventoryReportQuery query,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken);
        return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result);
    }
}

