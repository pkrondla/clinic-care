using MediatR;
using Microsoft.AspNetCore.Authorization;
using ClinicCare.Application.Features.Invoices.Queries.GetInvoices;
using ClinicCare.Application.Features.Invoices.Queries.GetInvoice;
using ClinicCare.Application.Features.Invoices.Queries.GetInvoicePdf;
using ClinicCare.Application.Features.Invoices.Commands.CreateInvoiceFromPrescription;
using ClinicCare.Application.Features.Invoices.Commands.InitiateOnlinePayment;
using ClinicCare.Application.Features.Invoices.Commands.PayInvoice;
using ClinicCare.Application.Features.Invoices.Commands.ProcessPaymentWebhook;
using ClinicCare.Application.Features.Invoices.Commands.UpdateCourierDocket;
using ClinicCare.Application.Common.Models;

namespace ClinicCare.API.Modules.Billing;

public static class BillingEndpoints
{
    public static IEndpointRouteBuilder MapBillingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/invoices")
            .WithTags("Invoices")
            .WithOpenApi()
            .RequireAuthorization();

        // Get all invoices
        group.MapGet("/", GetInvoices)
            .WithName("GetInvoices")
            .WithSummary("Get all invoices with optional filtering")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Get specific invoice
        group.MapGet("/{id:int}", GetInvoice)
            .WithName("GetInvoice")
            .WithSummary("Get a specific invoice by ID")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status404NotFound);

        // Create invoice from prescription
        group.MapPost("/from-prescription", CreateInvoiceFromPrescription)
            .WithName("CreateInvoiceFromPrescription")
            .WithSummary("Create an invoice from a prescription")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Pay invoice
        group.MapPost("/{id:int}/pay", PayInvoice)
            .WithName("PayInvoice")
            .WithSummary("Process payment for an invoice")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Get invoice PDF
        group.MapGet("/{id:int}/pdf", GetInvoicePdf)
            .WithName("GetInvoicePdf")
            .WithSummary("Get invoice as PDF")
            .Produces<byte[]>(StatusCodes.Status200OK, "application/pdf")
            .Produces<object>(StatusCodes.Status400BadRequest)
            .Produces<object>(StatusCodes.Status404NotFound);

        // Update courier docket
        group.MapPost("/{id:int}/courier", UpdateCourierDocket)
            .WithName("UpdateCourierDocket")
            .WithSummary("Update courier docket information for an invoice")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Online Payment Endpoints
        group.MapPost("/{id:int}/pay/online", InitiateOnlinePayment)
            .WithName("InitiateOnlinePayment")
            .WithSummary("Initiate online payment for an invoice")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest)
            .Produces<object>(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetInvoices(
        [AsParameters] GetInvoicesQuery query,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken);

        if (!result.Succeeded)
        {
            return Results.BadRequest(new { message = "Failed to retrieve invoices", errors = result.Errors });
        }

        return Results.Ok(new { data = result.Data });
    }

    private static async Task<IResult> GetInvoice(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetInvoiceQuery(id);
        var result = await mediator.Send(query, cancellationToken);

        if (!result.Succeeded)
        {
            return Results.NotFound(new { message = "Invoice not found", errors = result.Errors });
        }

        return Results.Ok(new { data = result.Data });
    }

    private static async Task<IResult> CreateInvoiceFromPrescription(
        CreateInvoiceFromPrescriptionCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        if (!result.Succeeded)
        {
            return Results.BadRequest(new { message = "Failed to create invoice", errors = result.Errors });
        }

        return Results.Ok(new { message = "Invoice created successfully", data = result.Data });
    }

    private static async Task<IResult> PayInvoice(
        int id,
        PayInvoiceCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        if (id != command.InvoiceId)
        {
            return Results.BadRequest(new { message = "ID mismatch" });
        }

        var result = await mediator.Send(command, cancellationToken);

        if (!result.Succeeded)
        {
            return Results.BadRequest(new { message = "Failed to process payment", errors = result.Errors });
        }

        return Results.Ok(new { message = "Payment processed successfully", data = result.Data });
    }

    private static async Task<IResult> GetInvoicePdf(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetInvoicePdfQuery(id);
        var result = await mediator.Send(query, cancellationToken);

        if (!result.Succeeded)
        {
            return Results.NotFound(new { message = "Invoice not found or failed to generate PDF", errors = result.Errors });
        }

        return Results.File(
            result.Data!,
            contentType: "application/pdf",
            fileDownloadName: $"Invoice_{id}.pdf"
        );
    }

    private static async Task<IResult> UpdateCourierDocket(
        int id,
        UpdateCourierDocketCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        if (id != command.InvoiceId)
        {
            return Results.BadRequest(new { message = "ID mismatch" });
        }

        var result = await mediator.Send(command, cancellationToken);

        if (!result.Succeeded)
        {
            return Results.BadRequest(new { message = "Failed to update courier docket", errors = result.Errors });
        }

        return Results.Ok(new { message = "Courier docket updated successfully", data = result.Data });
    }

    private static async Task<IResult> InitiateOnlinePayment(
        int id,
        InitiateOnlinePaymentCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        if (id != command.InvoiceId)
        {
            return Results.BadRequest(new { message = "ID mismatch" });
        }

        var result = await mediator.Send(command, cancellationToken);

        if (!result.Succeeded)
        {
            return Results.BadRequest(new { message = "Failed to initiate online payment", errors = result.Errors });
        }

        return Results.Ok(new { message = "Payment initiated successfully", data = result.Data });
    }
}

