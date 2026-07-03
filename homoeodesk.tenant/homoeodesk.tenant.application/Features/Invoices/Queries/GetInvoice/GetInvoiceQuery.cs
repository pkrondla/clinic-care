using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Invoices.Commands.CreateInvoiceFromPrescription;
using MediatR;

namespace HomoeoDesk.Tenant.Application.Features.Invoices.Queries.GetInvoice;

public record GetInvoiceQuery(int Id) : IRequest<Result<InvoiceDto>>;

