using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Invoices.Commands.CreateInvoiceFromPrescription;
using MediatR;

namespace ClinicCare.Application.Features.Invoices.Queries.GetInvoices;

public record GetInvoicesQuery(
    int? ClinicId = null,
    int? PatientId = null,
    int? Status = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<Result<List<InvoiceDto>>>;

