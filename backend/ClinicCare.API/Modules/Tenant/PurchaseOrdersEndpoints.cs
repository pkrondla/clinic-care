using ClinicCare.Application.Features.PurchaseOrders.Commands.ApprovePurchaseOrder;
using ClinicCare.Application.Features.PurchaseOrders.Commands.CancelPurchaseOrder;
using ClinicCare.Application.Features.PurchaseOrders.Commands.CreatePurchaseOrder;
using ClinicCare.Application.Features.PurchaseOrders.Commands.ReceivePurchaseOrder;
using ClinicCare.Application.Features.PurchaseOrders.Queries.GetPurchaseOrder;
using ClinicCare.Application.Features.PurchaseOrders.Queries.GetPurchaseOrders;
using ClinicCare.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace ClinicCare.API.Modules.Tenant;

public static class PurchaseOrdersEndpoints
{
    public static IEndpointRouteBuilder MapPurchaseOrdersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/purchase-orders")
            .WithTags("PurchaseOrders")
            .WithOpenApi()
            .RequireAuthorization();

        // Get all purchase orders
        group.MapGet("/", GetPurchaseOrders)
            .WithName("GetPurchaseOrders")
            .WithSummary("Get all purchase orders with optional filtering")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Get specific purchase order
        group.MapGet("/{id:int}", GetPurchaseOrder)
            .WithName("GetPurchaseOrder")
            .WithSummary("Get a specific purchase order by ID")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status404NotFound);

        // Create purchase order
        group.MapPost("/", CreatePurchaseOrder)
            .WithName("CreatePurchaseOrder")
            .WithSummary("Create a new purchase order")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Approve purchase order
        group.MapPost("/{id:int}/approve", ApprovePurchaseOrder)
            .WithName("ApprovePurchaseOrder")
            .WithSummary("Approve a purchase order")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Receive purchase order
        group.MapPost("/{id:int}/receive", ReceivePurchaseOrder)
            .WithName("ReceivePurchaseOrder")
            .WithSummary("Receive items from a purchase order")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Cancel purchase order
        group.MapPost("/{id:int}/cancel", CancelPurchaseOrder)
            .WithName("CancelPurchaseOrder")
            .WithSummary("Cancel a purchase order")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IResult> GetPurchaseOrders(
        int? clinicId,
        int? supplierId,
        int? status,
        DateTime? startDate,
        DateTime? endDate,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetPurchaseOrdersQuery
        {
            ClinicId = clinicId,
            SupplierId = supplierId,
            Status = status,
            StartDate = startDate,
            EndDate = endDate
        };
        var result = await mediator.Send(query, cancellationToken);

        if (!result.Succeeded)
        {
            return Results.BadRequest(new { message = "Failed to retrieve purchase orders", errors = result.Errors });
        }

        return Results.Ok(new { data = result.Data });
    }

    private static async Task<IResult> GetPurchaseOrder(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetPurchaseOrderQuery { Id = id };
        var result = await mediator.Send(query, cancellationToken);

        if (!result.Succeeded)
        {
            return Results.NotFound(new { message = "Purchase order not found", errors = result.Errors });
        }

        return Results.Ok(new { data = result.Data });
    }

    private static async Task<IResult> CreatePurchaseOrder(
        CreatePurchaseOrderCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        if (!result.Succeeded)
        {
            return Results.BadRequest(new { message = "Failed to create purchase order", errors = result.Errors });
        }

        return Results.Ok(new { message = "Purchase order created successfully", data = result.Data });
    }

    private static async Task<IResult> ApprovePurchaseOrder(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new ApprovePurchaseOrderCommand { Id = id };
        var result = await mediator.Send(command, cancellationToken);

        if (!result.Succeeded)
        {
            return Results.BadRequest(new { message = "Failed to approve purchase order", errors = result.Errors });
        }

        return Results.Ok(new { message = "Purchase order approved successfully", data = result.Data });
    }

    private static async Task<IResult> ReceivePurchaseOrder(
        int id,
        ReceivePurchaseOrderCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return Results.BadRequest(new { message = "ID mismatch" });
        }

        var result = await mediator.Send(command, cancellationToken);

        if (!result.Succeeded)
        {
            return Results.BadRequest(new { message = "Failed to receive purchase order", errors = result.Errors });
        }

        return Results.Ok(new { message = "Purchase order received successfully", data = result.Data });
    }

    private static async Task<IResult> CancelPurchaseOrder(
        int id,
        CancelPurchaseOrderCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return Results.BadRequest(new { message = "ID mismatch" });
        }

        var result = await mediator.Send(command, cancellationToken);

        if (!result.Succeeded)
        {
            return Results.BadRequest(new { message = "Failed to cancel purchase order", errors = result.Errors });
        }

        return Results.Ok(new { message = "Purchase order cancelled successfully", data = result.Data });
    }
}

