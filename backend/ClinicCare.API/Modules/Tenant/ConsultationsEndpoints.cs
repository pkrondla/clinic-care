using ClinicCare.Application.Features.Consultations.Commands.CreateConsultation;
using ClinicCare.Application.Features.Consultations.Commands.UpdateConsultation;
using ClinicCare.Application.Features.Consultations.Queries.GetConsultation;
using ClinicCare.Application.Features.Consultations.Queries.GetPatientConsultations;
using MediatR;

namespace ClinicCare.API.Modules.Tenant;

public static class ConsultationsEndpoints
{
    public static IEndpointRouteBuilder MapConsultationsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/consultations")
            .WithTags("Consultations")
            .WithOpenApi()
            .RequireAuthorization();

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

        return app;
    }

    private static async Task<IResult> GetConsultation(IMediator mediator, int id)
    {
        var query = new GetConsultationQuery { Id = id };
        var result = await mediator.Send(query);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data })
            : Results.NotFound(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> GetPatientConsultations(IMediator mediator, int patientId)
    {
        var query = new GetPatientConsultationsQuery { PatientId = patientId };
        var result = await mediator.Send(query);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> CreateConsultation(IMediator mediator, CreateConsultationCommand command)
    {
        var result = await mediator.Send(command);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data, message = "Consultation created successfully" })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> UpdateConsultation(IMediator mediator, int id, UpdateConsultationCommand command)
    {
        command.Id = id;
        var result = await mediator.Send(command);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data, message = "Consultation updated successfully" })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }
}

