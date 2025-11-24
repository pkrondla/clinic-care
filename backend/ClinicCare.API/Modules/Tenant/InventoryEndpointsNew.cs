using ClinicCare.Application.Features.Inventory.Commands.AdjustStock;
using ClinicCare.Application.Features.Inventory.Commands.CreateInventoryItem;
using ClinicCare.Application.Features.Inventory.Queries.GetInventory;
using ClinicCare.Application.Features.Inventory.Queries.GetLowStock;
using MediatR;

namespace ClinicCare.API.Modules.Tenant;

public static class InventoryEndpointsNew
{
    public static IEndpointRouteBuilder MapInventoryManagementEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inventory-management")
            .WithTags("Inventory Management")
            .WithOpenApi()
            .RequireAuthorization();

        // Get all inventory for clinic
        group.MapGet("/", GetInventory)
            .WithName("GetInventoryManagement")
            .WithSummary("Get all inventory items for current clinic")
            .Produces<object>(StatusCodes.Status200OK);

        // Get low stock items
        group.MapGet("/low-stock", GetLowStock)
            .WithName("GetInventoryManagementLowStock")
            .WithSummary("Get inventory items below reorder level")
            .Produces<object>(StatusCodes.Status200OK);

        // Create inventory item
        group.MapPost("/", CreateInventoryItem)
            .WithName("CreateInventoryManagementItem")
            .WithSummary("Create new inventory item")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Adjust stock (add or subtract)
        group.MapPost("/adjust-stock", AdjustStock)
            .WithName("AdjustInventoryManagementStock")
            .WithSummary("Adjust stock quantity (purchase, sale, adjustment, etc.)")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IResult> GetInventory(IMediator mediator, int? clinicId = null)
    {
        var query = new GetInventoryQuery { ClinicId = clinicId };
        var result = await mediator.Send(query);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> GetLowStock(IMediator mediator, int? clinicId = null)
    {
        var query = new GetLowStockQuery { ClinicId = clinicId };
        var result = await mediator.Send(query);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> CreateInventoryItem(IMediator mediator, CreateInventoryItemCommand command)
    {
        var result = await mediator.Send(command);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data, message = "Inventory item created successfully" })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> AdjustStock(IMediator mediator, AdjustStockCommand command)
    {
        var result = await mediator.Send(command);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data, message = "Stock adjusted successfully" })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }
}

