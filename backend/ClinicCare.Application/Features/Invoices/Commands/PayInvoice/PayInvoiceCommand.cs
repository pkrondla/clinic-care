using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Invoices.Commands.CreateInvoiceFromPrescription;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace ClinicCare.Application.Features.Invoices.Commands.PayInvoice;

public record PayInvoiceCommand(
    int InvoiceId,
    [Required] decimal Amount,
    [Required] string PaymentMethod,
    string? PaymentReference = null
) : IRequest<Result<InvoiceDto>>;

