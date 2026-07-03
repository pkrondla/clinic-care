using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Invoices.Commands.CreateInvoiceFromPrescription;

namespace HomoeoDesk.Tenant.Application.Common.Services;

/// <summary>
/// Applies a payment to an invoice. Shared by PayInvoiceHandler (authenticated request) and
/// ProcessPaymentWebhookHandler (anonymous gateway callback) so the webhook path doesn't have
/// to build and dispatch a PayInvoiceCommand through MediatR.
/// </summary>
public interface IInvoicePaymentService
{
    Task<Result<InvoiceDto>> ApplyPaymentAsync(
        int invoiceId,
        decimal amount,
        string paymentMethod,
        string? paymentReference,
        CancellationToken cancellationToken);
}
