using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Invoices.Commands.CreateInvoiceFromPrescription;
using MediatR;

namespace HomoeoDesk.Tenant.Application.Features.Invoices.Queries.GetInvoices;

public record GetInvoicesQuery(
    int? BranchId = null,
    int? PatientId = null,
    int? Status = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<Result<List<InvoiceDto>>>;

