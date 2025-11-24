using MediatR;
using Microsoft.AspNetCore.Authorization;
using ClinicCare.Application.Features.Appointments.Commands.CreateAppointment;
using ClinicCare.Application.Features.Appointments.Commands.UpdateAppointment;
using ClinicCare.Application.Features.Appointments.Commands.CancelAppointment;
using ClinicCare.Application.Features.Appointments.Queries.GetAppointments;
using ClinicCare.Application.Features.Appointments.Queries.GetAppointment;
using ClinicCare.Application.Features.Appointments.Queries.GetAppointmentStats;
using ClinicCare.Application.Features.Appointments.Queries.GetAllQueues;
using ClinicCare.Application.Features.Appointments.Queries.GetQueue;
using ClinicCare.Application.Features.Appointments.Commands.BookAppointment;
using ClinicCare.Application.Features.Appointments.Commands.StartAppointment;
using ClinicCare.Application.Features.Appointments.Commands.CompleteAppointment;
using ClinicCare.Application.Common.Models;

namespace ClinicCare.API.Modules.Appointments;

public static class AppointmentsEndpoints
{
    public static IEndpointRouteBuilder MapAppointmentsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/appointments")
            .WithTags("Appointments")
            .WithOpenApi()
            .RequireAuthorization();

        // Create appointment
        group.MapPost("/", CreateAppointment)
            .WithName("CreateAppointment")
            .WithSummary("Create a new appointment")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Get appointments with filtering
        group.MapGet("/", GetAppointments)
            .WithName("GetAppointments")
            .WithSummary("Get appointments with optional filtering")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Get specific appointment
        group.MapGet("/{id:int}", GetAppointment)
            .WithName("GetAppointment")
            .WithSummary("Get a specific appointment by ID")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status404NotFound);

        // Update appointment
        group.MapPut("/{id:int}", UpdateAppointment)
            .WithName("UpdateAppointment")
            .WithSummary("Update an appointment")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Cancel appointment
        group.MapDelete("/{id:int}", CancelAppointment)
            .WithName("CancelAppointment")
            .WithSummary("Cancel an appointment")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Get appointment statistics
        group.MapGet("/stats", GetAppointmentStats)
            .WithName("GetAppointmentStats")
            .WithSummary("Get appointment statistics")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Queue endpoints (authenticated)
        group.MapGet("/queues", GetAllQueues)
            .WithName("GetAllQueues")
            .WithSummary("Get all doctor queues (authenticated)")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        group.MapGet("/queues/{doctorId:int}", GetQueue)
            .WithName("GetQueue")
            .WithSummary("Get queue for a specific doctor")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest)
            .Produces<object>(StatusCodes.Status404NotFound);

        // Patient self-booking
        group.MapPost("/book", BookAppointment)
            .WithName("BookAppointment")
            .WithSummary("Patient self-booking (authenticated)")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Doctor queue processing
        group.MapPost("/{id:int}/start", StartAppointment)
            .WithName("StartAppointment")
            .WithSummary("Start an appointment (change status to In Progress)")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:int}/complete", CompleteAppointment)
            .WithName("CompleteAppointment")
            .WithSummary("Complete an appointment (change status to Completed)")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        return app;
    }

    // Public queue endpoints (no auth required)
    public static IEndpointRouteBuilder MapPublicQueueEndpoints(this IEndpointRouteBuilder app)
    {
        var publicGroup = app.MapGroup("/api/public/queues")
            .WithTags("Public Queues")
            .WithOpenApi();

        publicGroup.MapGet("/", GetPublicQueues)
            .WithName("GetPublicQueues")
            .WithSummary("Get all doctor queues (public - token numbers only)")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        publicGroup.MapGet("/{doctorId:int}", GetPublicQueue)
            .WithName("GetPublicQueue")
            .WithSummary("Get queue for a specific doctor (public - token numbers only)")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest)
            .Produces<object>(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> CreateAppointment(
        CreateAppointmentCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        
        if (!result.Succeeded)
        {
            return Results.BadRequest(new { message = "Failed to create appointment", errors = result.Errors });
        }

        return Results.Ok(new { message = "Appointment created successfully", data = result.Data });
    }

    private static async Task<IResult> GetAppointments(
        int? clinicId,
        int? doctorId,
        DateOnly? date,
        int? status,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetAppointmentsQuery(clinicId, doctorId, date, status);
        var result = await mediator.Send(query, cancellationToken);
        
        if (!result.Succeeded)
        {
            return Results.BadRequest(new { message = "Failed to retrieve appointments", errors = result.Errors });
        }

        return Results.Ok(new { data = result.Data });
    }

    private static async Task<IResult> GetAppointment(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetAppointmentQuery(id);
        var result = await mediator.Send(query, cancellationToken);
        
        if (!result.Succeeded)
        {
            return Results.NotFound(new { message = "Appointment not found", errors = result.Errors });
        }

        return Results.Ok(new { data = result.Data });
    }

    private static async Task<IResult> UpdateAppointment(
        int id,
        UpdateAppointmentCommand command,
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
            return Results.BadRequest(new { message = "Failed to update appointment", errors = result.Errors });
        }

        return Results.Ok(new { message = "Appointment updated successfully", data = result.Data });
    }

    private static async Task<IResult> CancelAppointment(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CancelAppointmentCommand(id);
        var result = await mediator.Send(command, cancellationToken);
        
        if (!result.Succeeded)
        {
            return Results.BadRequest(new { message = "Failed to cancel appointment", errors = result.Errors });
        }

        return Results.Ok(new { message = "Appointment cancelled successfully" });
    }

    private static async Task<IResult> GetAppointmentStats(
        int? clinicId,
        int? doctorId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetAppointmentStatsQuery(clinicId, doctorId);
        var result = await mediator.Send(query, cancellationToken);
        
        if (!result.Succeeded)
        {
            return Results.BadRequest(new { message = "Failed to retrieve appointment statistics", errors = result.Errors });
        }

        return Results.Ok(new { data = result.Data });
    }

    private static async Task<IResult> GetAllQueues(
        [AsParameters] GetAllQueuesQuery query,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken);
        
        if (!result.Succeeded)
        {
            return Results.BadRequest(new { message = "Failed to retrieve queues", errors = result.Errors });
        }

        return Results.Ok(new { data = result.Data });
    }

    private static async Task<IResult> GetQueue(
        int doctorId,
        [AsParameters] GetQueueQuery query,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        query.DoctorId = doctorId;
        var result = await mediator.Send(query, cancellationToken);
        
        if (!result.Succeeded)
        {
            if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                return Results.NotFound(new { message = "Queue not found", errors = result.Errors });
            }
            return Results.BadRequest(new { message = "Failed to retrieve queue", errors = result.Errors });
        }

        return Results.Ok(new { data = result.Data });
    }

    private static async Task<IResult> BookAppointment(
        BookAppointmentCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        
        if (!result.Succeeded)
        {
            return Results.BadRequest(new { message = "Failed to book appointment", errors = result.Errors });
        }

        return Results.Ok(new { message = "Appointment booked successfully", data = result.Data });
    }

    private static async Task<IResult> GetPublicQueues(
        [AsParameters] GetAllQueuesQuery query,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        // Force IncludePatientDetails to false for public view
        query.IncludePatientDetails = false;
        var result = await mediator.Send(query, cancellationToken);
        
        if (!result.Succeeded)
        {
            return Results.BadRequest(new { message = "Failed to retrieve queues", errors = result.Errors });
        }

        return Results.Ok(new { data = result.Data });
    }

    private static async Task<IResult> GetPublicQueue(
        int doctorId,
        [AsParameters] GetQueueQuery query,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        query.DoctorId = doctorId;
        // Force IncludePatientDetails to false for public view
        query.IncludePatientDetails = false;
        var result = await mediator.Send(query, cancellationToken);
        
        if (!result.Succeeded)
        {
            if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                return Results.NotFound(new { message = "Queue not found", errors = result.Errors });
            }
            return Results.BadRequest(new { message = "Failed to retrieve queue", errors = result.Errors });
        }

        return Results.Ok(new { data = result.Data });
    }

    private static async Task<IResult> StartAppointment(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new StartAppointmentCommand { Id = id };
        var result = await mediator.Send(command, cancellationToken);
        
        if (!result.Succeeded)
        {
            return Results.BadRequest(new { message = "Failed to start appointment", errors = result.Errors });
        }

        return Results.Ok(new { message = "Appointment started", data = result.Data });
    }

    private static async Task<IResult> CompleteAppointment(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CompleteAppointmentCommand { Id = id };
        var result = await mediator.Send(command, cancellationToken);
        
        if (!result.Succeeded)
        {
            return Results.BadRequest(new { message = "Failed to complete appointment", errors = result.Errors });
        }

        return Results.Ok(new { message = "Appointment completed", data = result.Data });
    }
}

