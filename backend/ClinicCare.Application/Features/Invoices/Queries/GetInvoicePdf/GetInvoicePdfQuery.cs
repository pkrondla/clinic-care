using ClinicCare.Application.Common.Models;
using MediatR;

namespace ClinicCare.Application.Features.Invoices.Queries.GetInvoicePdf;

public record GetInvoicePdfQuery(int InvoiceId) : IRequest<Result<byte[]>>;

