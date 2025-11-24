using ClinicCare.Application.Common.Models;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace ClinicCare.Application.Features.Invoices.Commands.ProcessPaymentWebhook;

public record ProcessPaymentWebhookCommand(
    [Required] string Payload,
    [Required] string Signature,
    string? GatewayName = null
) : IRequest<Result<PaymentWebhookProcessResultDto>>;

public class PaymentWebhookProcessResultDto
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public string? InvoiceId { get; set; }
    public string? Status { get; set; }
    public string? Message { get; set; }
}

