using HomoeoDesk.Tenant.Application.Features.Patients.Commands.CreatePatient;
using HomoeoDesk.Tenant.Application.Features.Patients.Commands.DeletePatient;
using HomoeoDesk.Tenant.Application.Features.Patients.Commands.UpdatePatient;
using HomoeoDesk.Tenant.Application.Features.Patients.Queries.GetPatient;
using HomoeoDesk.Tenant.Application.Features.Patients.Queries.GetPatients;
using HomoeoDesk.Tenant.Application.Features.Patients.Queries.SearchPatients;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace HomoeoDesk.Tenant.Api.Modules.Patients;

public static class PatientsEndpoints
{
    public static IEndpointRouteBuilder MapPatientsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/patients")
            .WithTags("Patients")
            .WithOpenApi()
            .RequireAuthorization();

        // Get all patients with filtering and pagination
        group.MapGet("/", GetPatients)
            .WithName("GetPatients")
            .WithSummary("Get all patients with filtering and pagination")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Search patients for quick lookup
        group.MapGet("/search", SearchPatients)
            .WithName("SearchPatients")
            .WithSummary("Search patients by name, email, phone, or patient code")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Get specific patient by ID
        group.MapGet("/{id:int}", GetPatient)
            .WithName("GetPatient")
            .WithSummary("Get a specific patient by ID with detailed information")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status404NotFound);

        // Create new patient
        group.MapPost("/", CreatePatient)
            .WithName("CreatePatient")
            .WithSummary("Create a new patient")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Update patient
        group.MapPut("/{id:int}", UpdatePatient)
            .WithName("UpdatePatient")
            .WithSummary("Update a patient")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest)
            .Produces<object>(StatusCodes.Status404NotFound);

        // Delete patient (soft delete)
        group.MapDelete("/{id:int}", DeletePatient)
            .WithName("DeletePatient")
            .WithSummary("Delete a patient (soft delete)")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest)
            .Produces<object>(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetPatients(
        [AsParameters] GetPatientsQuery query,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken);
        
        if (!result.Succeeded)
        {
            return Results.BadRequest(new { message = "Failed to retrieve patients", errors = result.Errors });
        }

        return Results.Ok(new { message = "Patients retrieved successfully", data = result.Data });
    }

    private static async Task<IResult> SearchPatients(
        [AsParameters] SearchPatientsQuery query,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken);
        
        if (!result.Succeeded)
        {
            return Results.BadRequest(new { message = "Failed to search patients", errors = result.Errors });
        }

        return Results.Ok(new { message = "Patients search completed", data = result.Data });
    }

    private static async Task<IResult> GetPatient(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetPatientQuery { Id = id };
            var result = await mediator.Send(query, cancellationToken);
            
            if (!result.Succeeded)
            {
                // Check if it's a "not found" error
                if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
                {
                    return Results.NotFound(new { message = "Patient not found", errors = result.Errors });
                }
                // Otherwise return 400 for other errors
                return Results.BadRequest(new { message = "Failed to retrieve patient", errors = result.Errors });
            }

            return Results.Ok(new { message = "Patient retrieved successfully", data = result.Data });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { message = "An error occurred while retrieving the patient", errors = new[] { ex.Message } });
        }
    }

    private static async Task<IResult> CreatePatient(
        CreatePatientCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        
        if (!result.Succeeded)
        {
            return Results.BadRequest(new { message = "Failed to create patient", errors = result.Errors });
        }

        return Results.Ok(new { message = "Patient created successfully", data = result.Data });
    }

    private static async Task<IResult> UpdatePatient(
        int id,
        UpdatePatientCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        command.Id = id;
        var result = await mediator.Send(command, cancellationToken);
        
        if (!result.Succeeded)
        {
            if (result.Errors.Any(e => e.Contains("not found")))
            {
                return Results.NotFound(new { message = "Patient not found", errors = result.Errors });
            }
            return Results.BadRequest(new { message = "Failed to update patient", errors = result.Errors });
        }

        return Results.Ok(new { message = "Patient updated successfully", data = result.Data });
    }

    private static async Task<IResult> DeletePatient(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new DeletePatientCommand { Id = id };
        var result = await mediator.Send(command, cancellationToken);
        
        if (!result.Succeeded)
        {
            if (result.Errors.Any(e => e.Contains("not found")))
            {
                return Results.NotFound(new { message = "Patient not found", errors = result.Errors });
            }
            return Results.BadRequest(new { message = "Failed to delete patient", errors = result.Errors });
        }

        return Results.Ok(new { message = "Patient deleted successfully" });
    }
}
