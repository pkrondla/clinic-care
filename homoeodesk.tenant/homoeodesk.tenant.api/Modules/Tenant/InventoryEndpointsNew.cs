using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Features.Inventory.Commands.AdjustStock;
using HomoeoDesk.Tenant.Application.Features.Inventory.Commands.CreateInventoryItem;
using HomoeoDesk.Tenant.Application.Features.Inventory.Queries.GetInventory;
using HomoeoDesk.Tenant.Application.Features.Inventory.Queries.GetLowStock;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Api.Modules.Tenant;

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

    private static async Task<IResult> GetInventory(
        IMediator mediator,
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        CancellationToken cancellationToken,
        int? BranchId = null)
    {
        // If BranchId not provided, get from current user's selected clinic
        if (!BranchId.HasValue && currentUserService.UserId.HasValue)
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == currentUserService.UserId.Value, cancellationToken);
            if (user?.SelectedBranchId.HasValue == true)
            {
                BranchId = user.SelectedBranchId;
            }
        }

        var query = new GetInventoryQuery { BranchId = BranchId };
        var result = await mediator.Send(query, cancellationToken);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> GetLowStock(
        IMediator mediator,
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        CancellationToken cancellationToken,
        int? BranchId = null)
    {
        // If BranchId not provided, get from current user's selected clinic
        if (!BranchId.HasValue && currentUserService.UserId.HasValue)
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == currentUserService.UserId.Value, cancellationToken);
            if (user?.SelectedBranchId.HasValue == true)
            {
                BranchId = user.SelectedBranchId;
            }
        }

        var query = new GetLowStockQuery { BranchId = BranchId };
        var result = await mediator.Send(query, cancellationToken);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> CreateInventoryItem(IMediator mediator, CreateInventoryItemCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data, message = "Inventory item created successfully" })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> AdjustStock(IMediator mediator, AdjustStockCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data, message = "Stock adjusted successfully" })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }
}

