using HomoeoDesk.Tenant.Application.Common.Models;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace HomoeoDesk.Tenant.Application.Features.Invoices.Commands.InitiateOnlinePayment;

public record InitiateOnlinePaymentCommand(
    [Required] int InvoiceId,
    [Required] string ReturnUrl,
    string? CancelUrl = null
) : IRequest<Result<OnlinePaymentInitiationDto>>;

public class OnlinePaymentInitiationDto
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public string PaymentUrl { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
    public Dictionary<string, string> AdditionalData { get; set; } = new();
}

