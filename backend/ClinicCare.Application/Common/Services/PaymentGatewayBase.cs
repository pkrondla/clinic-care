using ClinicCare.Application.Common.Services;
using Microsoft.Extensions.Logging;

namespace ClinicCare.Application.Common.Services;

/// <summary>
/// Base class for payment gateway implementations
/// Provides common functionality and helper methods
/// </summary>
public abstract class PaymentGatewayBase : IPaymentGateway
{
    protected readonly ILogger<PaymentGatewayBase> _logger;

    protected PaymentGatewayBase(ILogger<PaymentGatewayBase> logger)
    {
        _logger = logger;
    }

    public abstract Task<PaymentInitiationResult> InitiatePaymentAsync(
        PaymentInitiationRequest request, 
        CancellationToken cancellationToken = default);

    public abstract Task<PaymentStatusResult> VerifyPaymentAsync(
        string transactionId, 
        CancellationToken cancellationToken = default);

    public abstract Task<PaymentWebhookResult> ProcessWebhookAsync(
        string payload, 
        string signature, 
        CancellationToken cancellationToken = default);

    public abstract Task<PaymentRefundResult> RefundPaymentAsync(
        string transactionId, 
        decimal amount, 
        string reason, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate payment initiation request
    /// </summary>
    protected virtual bool ValidatePaymentRequest(PaymentInitiationRequest request, out string? errorMessage)
    {
        if (request == null)
        {
            errorMessage = "Payment request cannot be null";
            return false;
        }

        if (request.Amount <= 0)
        {
            errorMessage = "Payment amount must be greater than zero";
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.InvoiceNumber))
        {
            errorMessage = "Invoice number is required";
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.CustomerEmail) && string.IsNullOrWhiteSpace(request.CustomerPhone))
        {
            errorMessage = "Customer email or phone is required";
            return false;
        }

        errorMessage = null;
        return true;
    }

    /// <summary>
    /// Log payment operation
    /// </summary>
    protected virtual void LogPaymentOperation(string operation, string transactionId, object? data = null)
    {
        _logger.LogInformation(
            "Payment Gateway Operation: {Operation}, TransactionId: {TransactionId}, Data: {@Data}",
            operation, transactionId, data);
    }

    /// <summary>
    /// Log payment error
    /// </summary>
    protected virtual void LogPaymentError(string operation, string transactionId, Exception exception)
    {
        _logger.LogError(
            exception,
            "Payment Gateway Error: {Operation}, TransactionId: {TransactionId}",
            operation, transactionId);
    }
}

