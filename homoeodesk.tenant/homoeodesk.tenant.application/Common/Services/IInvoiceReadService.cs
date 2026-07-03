using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Invoices.Commands.CreateInvoiceFromPrescription;

namespace HomoeoDesk.Tenant.Application.Common.Services;

/// <summary>
/// Builds the full InvoiceDto for a given invoice. Shared by GetInvoiceHandler and
/// IInvoicePaymentService so neither has to round-trip through MediatR to fetch the other's DTO.
/// </summary>
public interface IInvoiceReadService
{
    Task<Result<InvoiceDto>> GetInvoiceDtoAsync(int invoiceId, CancellationToken cancellationToken);
}
