using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Invoices.Commands.CreateInvoiceFromPrescription;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace HomoeoDesk.Tenant.Application.Features.Invoices.Commands.UpdateCourierDocket;

public record UpdateCourierDocketCommand(
    int InvoiceId,
    [Required] string CourierDocketNumber,
    [Required] string CourierCompany,
    string? CourierTrackingUrl = null
) : IRequest<Result<InvoiceDto>>;

