using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Invoices.Commands.CreateInvoiceFromPrescription;
using MediatR;

namespace ClinicCare.Application.Features.Invoices.Queries.GetInvoice;

public record GetInvoiceQuery(int Id) : IRequest<Result<InvoiceDto>>;

