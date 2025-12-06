using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Common.Services;
using ClinicCare.Application.Features.Invoices.Commands.PayInvoice;
using ClinicCare.Domain.Enums;
using PaymentStatus = ClinicCare.Application.Common.Services.PaymentStatus;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.Invoices.Commands.ProcessPaymentWebhook;

public class ProcessPaymentWebhookHandler : IRequestHandler<ProcessPaymentWebhookCommand, Result<PaymentWebhookProcessResultDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPaymentGatewayFactory _paymentGatewayFactory;
    private readonly IMediator _mediator;

    public ProcessPaymentWebhookHandler(
        IApplicationDbContext context,
        IPaymentGatewayFactory paymentGatewayFactory,
        IMediator mediator)
    {
        _context = context;
        _paymentGatewayFactory = paymentGatewayFactory;
        _mediator = mediator;
    }

    public async Task<Result<PaymentWebhookProcessResultDto>> Handle(ProcessPaymentWebhookCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get payment gateway
            var paymentGateway = string.IsNullOrWhiteSpace(request.GatewayName)
                ? _paymentGatewayFactory.GetPaymentGateway()
                : _paymentGatewayFactory.GetPaymentGateway(request.GatewayName);

            // Process webhook
            var webhookResult = await paymentGateway.ProcessWebhookAsync(
                request.Payload, 
                request.Signature, 
                cancellationToken);

            if (!webhookResult.Success)
            {
                return Result<PaymentWebhookProcessResultDto>.Failure(
                    webhookResult.ErrorMessage ?? "Failed to process payment webhook");
            }

            // Extract invoice ID from metadata or transaction
            var invoiceId = ExtractInvoiceIdFromWebhook(webhookResult);

            if (!invoiceId.HasValue)
            {
                return Result<PaymentWebhookProcessResultDto>.Failure("Could not determine invoice ID from webhook");
            }

            // Update invoice payment status if payment was successful
            if (webhookResult.Status == PaymentStatus.Success)
            {
                var invoice = await _context.Invoices
                    .FirstOrDefaultAsync(i => i.Id == invoiceId.Value && i.IsActive, cancellationToken);

                if (invoice != null && invoice.Status != InvoiceStatus.Paid)
                {
                    // Process payment
                    var payCommand = new PayInvoiceCommand(
                        invoiceId.Value,
                        webhookResult.Amount,
                        webhookResult.PaymentMethod ?? "Online",
                        webhookResult.TransactionId
                    );

                    await _mediator.Send(payCommand, cancellationToken);
                }
            }

            var result = new PaymentWebhookProcessResultDto
            {
                Success = true,
                TransactionId = webhookResult.TransactionId,
                InvoiceId = invoiceId.Value.ToString(),
                Status = webhookResult.Status.ToString(),
                Message = "Payment webhook processed successfully"
            };

            return Result<PaymentWebhookProcessResultDto>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<PaymentWebhookProcessResultDto>.Failure($"Failed to process payment webhook: {ex.Message}");
        }
    }

    private int? ExtractInvoiceIdFromWebhook(PaymentWebhookResult webhookResult)
    {
        // Try to extract from metadata
        if (webhookResult.AdditionalData.TryGetValue("InvoiceId", out var invoiceIdStr) 
            && int.TryParse(invoiceIdStr, out var invoiceId))
        {
            return invoiceId;
        }

        // Try to find invoice by transaction ID in payment reference
        if (!string.IsNullOrWhiteSpace(webhookResult.TransactionId))
        {
            var invoice = _context.Invoices
                .FirstOrDefault(i => i.PaymentReference == webhookResult.TransactionId && i.IsActive);
            
            if (invoice != null)
            {
                return invoice.Id;
            }
        }

        // In a real implementation, you might need to:
        // 1. Query the payment gateway API
        // 2. Maintain a transaction-to-invoice mapping table
        // 3. Parse the webhook payload more thoroughly
        return null;
    }
}

