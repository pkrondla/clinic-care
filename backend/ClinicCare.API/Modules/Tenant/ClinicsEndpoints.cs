using ClinicCare.Application.Features.Clinics.Commands.CreateClinic;
using ClinicCare.Application.Features.Clinics.Commands.UpdateClinic;
using ClinicCare.Application.Features.Clinics.Queries.GetClinic;
using ClinicCare.Application.Features.Clinics.Queries.GetClinics;
using MediatR;

namespace ClinicCare.API.Modules.Tenant;

public static class ClinicsEndpoints
{
    public static IEndpointRouteBuilder MapClinicsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/clinics")
            .WithTags("Clinics")
            .WithOpenApi()
            .RequireAuthorization();

        // Get all clinics
        group.MapGet("/", GetClinics)
            .WithName("GetClinics")
            .WithSummary("Get all clinics for current organization")
            .Produces<object>(StatusCodes.Status200OK);

        // Get clinic by ID
        group.MapGet("/{id:int}", GetClinic)
            .WithName("GetClinic")
            .WithSummary("Get clinic by ID")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status404NotFound);

        // Create clinic
        group.MapPost("/", CreateClinic)
            .WithName("CreateClinic")
            .WithSummary("Create a new clinic (Admin only)")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Update clinic
        group.MapPut("/{id:int}", UpdateClinic)
            .WithName("UpdateClinic")
            .WithSummary("Update clinic (Admin only)")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetClinics(IMediator mediator)
    {
        var query = new GetClinicsQuery();
        var result = await mediator.Send(query);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> GetClinic(IMediator mediator, int id)
    {
        var query = new GetClinicQuery { Id = id };
        var result = await mediator.Send(query);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data })
            : Results.NotFound(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> CreateClinic(IMediator mediator, CreateClinicCommand command)
    {
        var result = await mediator.Send(command);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data, message = "Clinic created successfully" })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> UpdateClinic(IMediator mediator, int id, UpdateClinicCommand command)
    {
        command.Id = id;
        var result = await mediator.Send(command);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data, message = "Clinic updated successfully" })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }
}

