using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Invoices.Commands.CreateInvoiceFromPrescription;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace ClinicCare.Application.Features.Invoices.Commands.UpdateCourierDocket;

public record UpdateCourierDocketCommand(
    int InvoiceId,
    [Required] string CourierDocketNumber,
    [Required] string CourierCompany,
    string? CourierTrackingUrl = null
) : IRequest<Result<InvoiceDto>>;

