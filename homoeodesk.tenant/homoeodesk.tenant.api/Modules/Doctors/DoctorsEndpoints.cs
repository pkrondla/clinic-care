using MediatR;
using HomoeoDesk.Tenant.Application.Features.Doctors.Queries.GetDoctors;
using HomoeoDesk.Tenant.Application.Features.Doctors.Queries.GetDoctorAvailability;
using HomoeoDesk.Tenant.Application.Features.Doctors.Commands.CreateDoctorAvailability;
using HomoeoDesk.Tenant.Application.Features.Doctors.Commands.UpdateDoctorAvailability;
using HomoeoDesk.Tenant.Application.Features.Doctors.Commands.DeleteDoctorAvailability;
using HomoeoDesk.Tenant.Application.Common.Models;

namespace HomoeoDesk.Tenant.Api.Modules.Doctors;

public static class DoctorsEndpoints
{
    public static IEndpointRouteBuilder MapDoctorsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/doctors")
            .WithTags("Doctors")
            .WithOpenApi()
            .RequireAuthorization();

        // Get doctors
        group.MapGet("/", GetDoctors)
            .WithName("GetDoctors")
            .WithSummary("Get list of doctors")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Doctor Availability Endpoints
        group.MapGet("/availability", GetDoctorAvailability)
            .WithName("GetDoctorAvailability")
            .WithSummary("Get doctor availability schedule")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        group.MapPost("/availability", CreateDoctorAvailability)
            .WithName("CreateDoctorAvailability")
            .WithSummary("Create doctor availability schedule")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        group.MapPut("/availability/{id:int}", UpdateDoctorAvailability)
            .WithName("UpdateDoctorAvailability")
            .WithSummary("Update doctor availability schedule")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest)
            .Produces<object>(StatusCodes.Status404NotFound);

        group.MapDelete("/availability/{id:int}", DeleteDoctorAvailability)
            .WithName("DeleteDoctorAvailability")
            .WithSummary("Delete doctor availability schedule")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest)
            .Produces<object>(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetDoctors(
        [AsParameters] GetDoctorsQuery query,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken);
        
        if (!result.Succeeded)
        {
            return Results.BadRequest(new { message = "Failed to retrieve doctors", errors = result.Errors });
        }

        return Results.Ok(new { data = result.Data });
    }

    private static async Task<IResult> GetDoctorAvailability(
        [AsParameters] GetDoctorAvailabilityQuery query,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken);
        return result.Succeeded ? Results.Ok(new { data = result.Data }) : Results.BadRequest(new { errors = result.Errors });
    }

    private static async Task<IResult> CreateDoctorAvailability(
        CreateDoctorAvailabilityCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return result.Succeeded 
            ? Results.Ok(new { message = "Doctor availability created successfully", data = result.Data })
            : Results.BadRequest(new { errors = result.Errors });
    }

    private static async Task<IResult> UpdateDoctorAvailability(
        int id,
        UpdateDoctorAvailabilityCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return Results.BadRequest(new { message = "ID mismatch" });
        }

        var result = await mediator.Send(command, cancellationToken);
        return result.Succeeded
            ? Results.Ok(new { message = "Doctor availability updated successfully", data = result.Data })
            : Results.BadRequest(new { errors = result.Errors });
    }

    private static async Task<IResult> DeleteDoctorAvailability(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new DeleteDoctorAvailabilityCommand(id);
        var result = await mediator.Send(command, cancellationToken);
        return result.Succeeded
            ? Results.Ok(new { message = "Doctor availability deleted successfully" })
            : Results.BadRequest(new { errors = result.Errors });
    }
}
