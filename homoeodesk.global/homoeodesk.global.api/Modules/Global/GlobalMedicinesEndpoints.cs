using HomoeoDesk.Global.Application.Features.GlobalMedicines.Commands.CreateGlobalMedicine;
using HomoeoDesk.Global.Application.Features.GlobalMedicines.Commands.DeleteGlobalMedicine;
using HomoeoDesk.Global.Application.Features.GlobalMedicines.Commands.UpdateGlobalMedicine;
using HomoeoDesk.Global.Application.Features.GlobalMedicines.Queries.GetGlobalMedicine;
using HomoeoDesk.Global.Application.Features.GlobalMedicines.Queries.GetGlobalMedicines;
using MediatR;

namespace HomoeoDesk.Global.Api.Modules.Global;

public static class GlobalMedicinesEndpoints
{
    public static IEndpointRouteBuilder MapGlobalMedicinesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/global/medicines")
            .WithTags("Global Medicines")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/", GetGlobalMedicines)
            .WithName("GetGlobalMedicines")
            .WithSummary("Get all global medicines with optional filtering")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:int}", GetGlobalMedicine)
            .WithName("GetGlobalMedicine")
            .WithSummary("Get a specific global medicine by ID")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateGlobalMedicine)
            .WithName("CreateGlobalMedicine")
            .WithSummary("Create a new global medicine (Super Admin only)")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:int}", UpdateGlobalMedicine)
            .WithName("UpdateGlobalMedicine")
            .WithSummary("Update an existing global medicine (Super Admin only)")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest)
            .Produces<object>(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}", DeleteGlobalMedicine)
            .WithName("DeleteGlobalMedicine")
            .WithSummary("Delete a global medicine (Super Admin only)")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetGlobalMedicines(
        IMediator mediator,
        string? searchTerm = null,
        string? type = null,
        string? manufacturer = null)
    {
        var result = await mediator.Send(new GetGlobalMedicinesQuery
        {
            SearchTerm = searchTerm,
            Type = type,
            Manufacturer = manufacturer
        });

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> GetGlobalMedicine(IMediator mediator, int id)
    {
        var result = await mediator.Send(new GetGlobalMedicineQuery { Id = id });

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data })
            : Results.NotFound(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> CreateGlobalMedicine(IMediator mediator, CreateGlobalMedicineCommand command)
    {
        var result = await mediator.Send(command);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data, message = "Global medicine created successfully" })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> UpdateGlobalMedicine(IMediator mediator, int id, UpdateGlobalMedicineCommand command)
    {
        command.Id = id;
        var result = await mediator.Send(command);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data, message = "Global medicine updated successfully" })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> DeleteGlobalMedicine(IMediator mediator, int id)
    {
        var result = await mediator.Send(new DeleteGlobalMedicineCommand { Id = id });

        return result.Succeeded
            ? Results.Ok(new { success = true, message = "Global medicine deleted successfully" })
            : Results.NotFound(new { success = false, errors = result.Errors });
    }
}
