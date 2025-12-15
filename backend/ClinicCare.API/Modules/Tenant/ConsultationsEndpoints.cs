using ClinicCare.Application.Features.Consultations.Commands.AddConsultationPhoto;
using ClinicCare.Application.Features.Consultations.Commands.CreateConsultation;
using ClinicCare.Application.Features.Consultations.Commands.DeleteConsultationPhoto;
using ClinicCare.Application.Features.Consultations.Commands.UpdateConsultation;
using ClinicCare.Application.Features.Consultations.Queries.GetConsultation;
using ClinicCare.Application.Features.Consultations.Queries.GetPatientConsultations;
using ClinicCare.Application.Features.Consultations.Queries.GetConsultations;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicCare.API.Modules.Tenant;

public static class ConsultationsEndpoints
{
    public static IEndpointRouteBuilder MapConsultationsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/consultations")
            .WithTags("Consultations")
            .WithOpenApi()
            .RequireAuthorization();

        // Get all consultations with optional filters
        group.MapGet("/", GetConsultations)
            .WithName("GetConsultations")
            .WithSummary("Get all consultations with optional filtering")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Get consultation by ID
        group.MapGet("/{id:int}", GetConsultation)
            .WithName("GetConsultation")
            .WithSummary("Get consultation by ID (with full details)")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status404NotFound);

        // Get all consultations for a patient (history)
        group.MapGet("/patient/{patientId:int}", GetPatientConsultations)
            .WithName("GetPatientConsultations")
            .WithSummary("Get all consultations for a patient")
            .Produces<object>(StatusCodes.Status200OK);

        // Create consultation
        group.MapPost("/", CreateConsultation)
            .WithName("CreateConsultation")
            .WithSummary("Create new consultation (Doctor only)")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Update consultation
        group.MapPut("/{id:int}", UpdateConsultation)
            .WithName("UpdateConsultation")
            .WithSummary("Update consultation (Doctor only)")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status404NotFound);

        // Add photo to consultation
        group.MapPost("/{consultationId:int}/photos", AddConsultationPhoto)
            .WithName("AddConsultationPhoto")
            .WithSummary("Add photo to consultation")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Delete photo from consultation
        group.MapDelete("/photos/{photoId:int}", DeleteConsultationPhoto)
            .WithName("DeleteConsultationPhoto")
            .WithSummary("Delete photo from consultation")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IResult> GetConsultations(
        IMediator mediator,
        CancellationToken cancellationToken,
        [FromQuery] int? clinicId,
        [FromQuery] int? doctorId,
        [FromQuery] int? patientId,
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate)
    {
        var query = new GetConsultationsQuery
        {
            ClinicId = clinicId,
            DoctorId = doctorId,
            PatientId = patientId,
            StartDate = startDate,
            EndDate = endDate
        };
        var result = await mediator.Send(query, cancellationToken);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> GetConsultation(IMediator mediator, int id, CancellationToken cancellationToken)
    {
        var query = new GetConsultationQuery { Id = id };
        var result = await mediator.Send(query, cancellationToken);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data })
            : Results.NotFound(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> GetPatientConsultations(IMediator mediator, int patientId, CancellationToken cancellationToken)
    {
        var query = new GetPatientConsultationsQuery { PatientId = patientId };
        var result = await mediator.Send(query, cancellationToken);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> CreateConsultation(IMediator mediator, CreateConsultationCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data, message = "Consultation created successfully" })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> UpdateConsultation(IMediator mediator, int id, UpdateConsultationCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        var result = await mediator.Send(command, cancellationToken);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data, message = "Consultation updated successfully" })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> AddConsultationPhoto(IMediator mediator, int consultationId, AddConsultationPhotoCommand command, CancellationToken cancellationToken)
    {
        command.ConsultationId = consultationId;
        var result = await mediator.Send(command, cancellationToken);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data, message = "Photo added successfully" })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> DeleteConsultationPhoto(IMediator mediator, int photoId, CancellationToken cancellationToken)
    {
        var command = new DeleteConsultationPhotoCommand { PhotoId = photoId };
        var result = await mediator.Send(command, cancellationToken);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data, message = "Photo deleted successfully" })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }
}

