using HomoeoDesk.Tenant.Application.Common.Models;
using MediatR;

namespace HomoeoDesk.Tenant.Application.Features.Invoices.Queries.GetInvoicePdf;

public record GetInvoicePdfQuery(int InvoiceId) : IRequest<Result<byte[]>>;

