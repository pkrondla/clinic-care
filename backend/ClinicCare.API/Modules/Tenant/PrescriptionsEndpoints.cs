using ClinicCare.Application.Features.Prescriptions.Commands.CreatePrescription;
using ClinicCare.Application.Features.Prescriptions.Queries.GetPatientPrescriptions;
using ClinicCare.Application.Features.Prescriptions.Queries.GetPrescription;
using ClinicCare.Application.Features.Prescriptions.Queries.GetPrescriptionPdf;
using ClinicCare.Application.Features.Prescriptions.Queries.GetPrescriptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicCare.API.Modules.Tenant;

public static class PrescriptionsEndpoints
{
    public static IEndpointRouteBuilder MapPrescriptionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/prescriptions")
            .WithTags("Prescriptions")
            .WithOpenApi()
            .RequireAuthorization();

        // Get all prescriptions with optional filters
        group.MapGet("/", GetPrescriptions)
            .WithName("GetPrescriptions")
            .WithSummary("Get all prescriptions with optional filtering")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Get prescription by ID
        group.MapGet("/{id:int}", GetPrescription)
            .WithName("GetPrescription")
            .WithSummary("Get prescription by ID (with medicines)")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status404NotFound);

        // Get all prescriptions for a patient
        group.MapGet("/patient/{patientId:int}", GetPatientPrescriptions)
            .WithName("GetPatientPrescriptions")
            .WithSummary("Get all prescriptions for a patient")
            .Produces<object>(StatusCodes.Status200OK);

        // Create prescription
        group.MapPost("/", CreatePrescription)
            .WithName("CreatePrescription")
            .WithSummary("Create new prescription (Doctor only)")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Get prescription PDF
        group.MapGet("/{id:int}/pdf", GetPrescriptionPdf)
            .WithName("GetPrescriptionPdf")
            .WithSummary("Get prescription as PDF")
            .Produces<byte[]>(StatusCodes.Status200OK, "application/pdf")
            .Produces<object>(StatusCodes.Status400BadRequest)
            .Produces<object>(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetPrescriptions(
        IMediator mediator,
        CancellationToken cancellationToken,
        [FromQuery] int? clinicId,
        [FromQuery] int? doctorId,
        [FromQuery] int? patientId,
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate)
    {
        var query = new GetPrescriptionsQuery
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

    private static async Task<IResult> GetPrescription(IMediator mediator, int id, CancellationToken cancellationToken)
    {
        var query = new GetPrescriptionQuery { Id = id };
        var result = await mediator.Send(query, cancellationToken);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data })
            : Results.NotFound(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> GetPatientPrescriptions(IMediator mediator, int patientId, CancellationToken cancellationToken)
    {
        var query = new GetPatientPrescriptionsQuery { PatientId = patientId };
        var result = await mediator.Send(query, cancellationToken);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> CreatePrescription(IMediator mediator, CreatePrescriptionCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data, message = "Prescription created successfully" })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> GetPrescriptionPdf(
        int id,
        bool? includeMedicineNames,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetPrescriptionPdfQuery(id, includeMedicineNames ?? true);
        var result = await mediator.Send(query, cancellationToken);

        if (!result.Succeeded)
        {
            return Results.NotFound(new { message = "Prescription not found or failed to generate PDF", errors = result.Errors });
        }

        var templateType = includeMedicineNames == false ? "Patient" : "Internal";
        return Results.File(
            result.Data!,
            contentType: "application/pdf",
            fileDownloadName: $"Prescription_{id}_{templateType}.pdf"
        );
    }
}

