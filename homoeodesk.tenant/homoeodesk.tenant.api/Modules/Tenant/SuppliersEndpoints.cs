using HomoeoDesk.Tenant.Application.Features.Suppliers.Commands.CreateSupplier;
using HomoeoDesk.Tenant.Application.Features.Suppliers.Commands.DeleteSupplier;
using HomoeoDesk.Tenant.Application.Features.Suppliers.Commands.UpdateSupplier;
using HomoeoDesk.Tenant.Application.Features.Suppliers.Queries.GetSupplier;
using HomoeoDesk.Tenant.Application.Features.Suppliers.Queries.GetSuppliers;
using HomoeoDesk.Tenant.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace HomoeoDesk.Tenant.Api.Modules.Tenant;

public static class SuppliersEndpoints
{
    public static IEndpointRouteBuilder MapSuppliersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/suppliers")
            .WithTags("Suppliers")
            .WithOpenApi()
            .RequireAuthorization();

        // Get all suppliers
        group.MapGet("/", GetSuppliers)
            .WithName("GetSuppliers")
            .WithSummary("Get all suppliers with optional filtering")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Get specific supplier
        group.MapGet("/{id:int}", GetSupplier)
            .WithName("GetSupplier")
            .WithSummary("Get a specific supplier by ID")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status404NotFound);

        // Create supplier
        group.MapPost("/", CreateSupplier)
            .WithName("CreateSupplier")
            .WithSummary("Create a new supplier")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Update supplier
        group.MapPut("/{id:int}", UpdateSupplier)
            .WithName("UpdateSupplier")
            .WithSummary("Update an existing supplier")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest)
            .Produces<object>(StatusCodes.Status404NotFound);

        // Delete supplier
        group.MapDelete("/{id:int}", DeleteSupplier)
            .WithName("DeleteSupplier")
            .WithSummary("Delete a supplier")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest)
            .Produces<object>(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetSuppliers(
        string? searchTerm,
        bool? isActive,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetSuppliersQuery
        {
            SearchTerm = searchTerm,
            IsActive = isActive
        };
        var result = await mediator.Send(query, cancellationToken);

        if (!result.Succeeded)
        {
            return Results.BadRequest(new { message = "Failed to retrieve suppliers", errors = result.Errors });
        }

        return Results.Ok(new { data = result.Data });
    }

    private static async Task<IResult> GetSupplier(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetSupplierQuery { Id = id };
        var result = await mediator.Send(query, cancellationToken);

        if (!result.Succeeded)
        {
            return Results.NotFound(new { message = "Supplier not found", errors = result.Errors });
        }

        return Results.Ok(new { data = result.Data });
    }

    private static async Task<IResult> CreateSupplier(
        CreateSupplierCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        if (!result.Succeeded)
        {
            return Results.BadRequest(new { message = "Failed to create supplier", errors = result.Errors });
        }

        return Results.Ok(new { message = "Supplier created successfully", data = result.Data });
    }

    private static async Task<IResult> UpdateSupplier(
        int id,
        UpdateSupplierCommand command,
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
            return Results.BadRequest(new { message = "Failed to update supplier", errors = result.Errors });
        }

        return Results.Ok(new { message = "Supplier updated successfully", data = result.Data });
    }

    private static async Task<IResult> DeleteSupplier(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new DeleteSupplierCommand { Id = id };
        var result = await mediator.Send(command, cancellationToken);

        if (!result.Succeeded)
        {
            return Results.BadRequest(new { message = "Failed to delete supplier", errors = result.Errors });
        }

        return Results.Ok(new { message = "Supplier deleted successfully" });
    }
}

