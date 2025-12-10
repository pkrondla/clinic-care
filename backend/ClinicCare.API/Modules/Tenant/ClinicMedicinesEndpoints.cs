using ClinicCare.Application.Features.ClinicMedicines.Commands.AddClinicMedicineFromGlobal;
using ClinicCare.Application.Features.ClinicMedicines.Commands.CreateClinicMedicine;
using ClinicCare.Application.Features.ClinicMedicines.Commands.DeleteClinicMedicine;
using ClinicCare.Application.Features.ClinicMedicines.Commands.UpdateClinicMedicine;
using ClinicCare.Application.Features.ClinicMedicines.Queries.GetClinicMedicine;
using ClinicCare.Application.Features.ClinicMedicines.Queries.GetClinicMedicines;
using ClinicCare.Application.Features.ClinicMedicines.Queries.SearchClinicMedicines;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicCare.API.Modules.Tenant;

public static class ClinicMedicinesEndpoints
{
    public static IEndpointRouteBuilder MapClinicMedicinesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/clinic-medicines")
            .WithTags("Clinic Medicines")
            .WithOpenApi()
            .RequireAuthorization();

        // Get all clinic medicines
        group.MapGet("/", GetClinicMedicines)
            .WithName("GetClinicMedicines")
            .WithSummary("Get all clinic medicines with optional filtering")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Get clinic medicine by ID
        group.MapGet("/{id:int}", GetClinicMedicine)
            .WithName("GetClinicMedicine")
            .WithSummary("Get clinic medicine by ID")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status404NotFound);

        // Search clinic medicines
        group.MapGet("/search", SearchClinicMedicines)
            .WithName("SearchClinicMedicines")
            .WithSummary("Search clinic medicines by name, generic name, manufacturer, or type")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Create clinic medicine
        group.MapPost("/", CreateClinicMedicine)
            .WithName("CreateClinicMedicine")
            .WithSummary("Create a new clinic medicine")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Add medicine from global catalog
        group.MapPost("/from-global", AddFromGlobal)
            .WithName("AddClinicMedicineFromGlobal")
            .WithSummary("Add a medicine from global catalog to clinic catalog")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Update clinic medicine
        group.MapPut("/{id:int}", UpdateClinicMedicine)
            .WithName("UpdateClinicMedicine")
            .WithSummary("Update an existing clinic medicine")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest)
            .Produces<object>(StatusCodes.Status404NotFound);

        // Delete clinic medicine (soft delete)
        group.MapDelete("/{id:int}", DeleteClinicMedicine)
            .WithName("DeleteClinicMedicine")
            .WithSummary("Delete a clinic medicine (soft delete)")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest)
            .Produces<object>(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetClinicMedicines(
        IMediator mediator,
        CancellationToken cancellationToken,
        [FromQuery] string? searchTerm,
        [FromQuery] int? clinicId,
        [FromQuery] bool? isActive)
    {
        var query = new GetClinicMedicinesQuery
        {
            SearchTerm = searchTerm,
            ClinicId = clinicId,
            IsActive = isActive
        };
        var result = await mediator.Send(query, cancellationToken);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> GetClinicMedicine(
        IMediator mediator,
        int id,
        CancellationToken cancellationToken)
    {
        var query = new GetClinicMedicineQuery { Id = id };
        var result = await mediator.Send(query, cancellationToken);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data })
            : Results.NotFound(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> SearchClinicMedicines(
        IMediator mediator,
        CancellationToken cancellationToken,
        [FromQuery] string? searchTerm)
    {
        var query = new SearchClinicMedicinesQuery { SearchTerm = searchTerm };
        var result = await mediator.Send(query, cancellationToken);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> CreateClinicMedicine(
        IMediator mediator,
        CreateClinicMedicineCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data, message = "Clinic medicine created successfully" })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> AddFromGlobal(
        IMediator mediator,
        AddClinicMedicineFromGlobalCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data, message = "Medicine added from global catalog successfully" })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> UpdateClinicMedicine(
        IMediator mediator,
        int id,
        UpdateClinicMedicineCommand command,
        CancellationToken cancellationToken)
    {
        command.Id = id;
        var result = await mediator.Send(command, cancellationToken);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data, message = "Clinic medicine updated successfully" })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> DeleteClinicMedicine(
        IMediator mediator,
        int id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteClinicMedicineCommand { Id = id };
        var result = await mediator.Send(command, cancellationToken);

        return result.Succeeded
            ? Results.Ok(new { success = true, message = "Clinic medicine deleted successfully" })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }
}

