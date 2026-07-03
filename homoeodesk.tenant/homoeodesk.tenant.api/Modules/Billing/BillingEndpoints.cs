using MediatR;
using Microsoft.AspNetCore.Authorization;
using HomoeoDesk.Tenant.Application.Features.Invoices.Queries.GetInvoices;
using HomoeoDesk.Tenant.Application.Features.Invoices.Queries.GetInvoice;
using HomoeoDesk.Tenant.Application.Features.Invoices.Queries.GetInvoicePdf;
using HomoeoDesk.Tenant.Application.Features.Invoices.Commands.CreateInvoiceFromPrescription;
using HomoeoDesk.Tenant.Application.Features.Invoices.Commands.CreateInvoice;
using HomoeoDesk.Tenant.Application.Features.Invoices.Commands.UpdateInvoice;
using HomoeoDesk.Tenant.Application.Features.Invoices.Commands.InitiateOnlinePayment;
using HomoeoDesk.Tenant.Application.Features.Invoices.Commands.PayInvoice;
using HomoeoDesk.Tenant.Application.Features.Invoices.Commands.ProcessPaymentWebhook;
using HomoeoDesk.Tenant.Application.Features.Invoices.Commands.UpdateCourierDocket;
using HomoeoDesk.Tenant.Application.Features.Invoices.Queries.PrepareInvoiceFromPrescription;
using HomoeoDesk.Tenant.Application.Common.Models;

namespace HomoeoDesk.Tenant.Api.Modules.Billing;

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

        // Prepare invoice from prescription (get invoice data without creating)
        // IMPORTANT: This must be registered BEFORE /{id:int} route to avoid route conflicts
        group.MapGet("/prepare-from-prescription/{prescriptionId:int}", PrepareInvoiceFromPrescription)
            .WithName("PrepareInvoiceFromPrescription")
            .WithSummary("Get invoice preparation data from a prescription (without creating invoice)")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest)
            .Produces<object>(StatusCodes.Status404NotFound);

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

        // Create invoice manually
        group.MapPost("/", CreateInvoice)
            .WithName("CreateInvoice")
            .WithSummary("Create a new invoice manually")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Update invoice
        group.MapPut("/{id:int}", UpdateInvoice)
            .WithName("UpdateInvoice")
            .WithSummary("Update an existing invoice")
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

    private static async Task<IResult> PrepareInvoiceFromPrescription(
        int prescriptionId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new PrepareInvoiceFromPrescriptionQuery(prescriptionId);
        var result = await mediator.Send(query, cancellationToken);

        if (!result.Succeeded)
        {
            if (result.Errors.Any(e => e.Contains("not found")))
            {
                return Results.NotFound(new { message = "Prescription not found", errors = result.Errors });
            }
            return Results.BadRequest(new { message = "Failed to prepare invoice", errors = result.Errors });
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

    private static async Task<IResult> CreateInvoice(
        CreateInvoiceCommand command,
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

    private static async Task<IResult> UpdateInvoice(
        int id,
        UpdateInvoiceCommand command,
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
            return Results.BadRequest(new { message = "Failed to update invoice", errors = result.Errors });
        }

        return Results.Ok(new { message = "Invoice updated successfully", data = result.Data });
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

