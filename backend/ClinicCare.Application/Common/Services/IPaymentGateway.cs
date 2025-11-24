namespace ClinicCare.Application.Common.Services;

/// <summary>
/// Generic payment gateway interface for processing online payments
/// </summary>
public interface IPaymentGateway
{
    /// <summary>
    /// Initialize a payment transaction
    /// </summary>
    Task<PaymentInitiationResult> InitiatePaymentAsync(PaymentInitiationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify payment status
    /// </summary>
    Task<PaymentStatusResult> VerifyPaymentAsync(string transactionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Process payment webhook callback
    /// </summary>
    Task<PaymentWebhookResult> ProcessWebhookAsync(string payload, string signature, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refund a payment
    /// </summary>
    Task<PaymentRefundResult> RefundPaymentAsync(string transactionId, decimal amount, string reason, CancellationToken cancellationToken = default);
}

/// <summary>
/// Payment initiation request
/// </summary>
public class PaymentInitiationRequest
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Payment initiation result
/// </summary>
public class PaymentInitiationResult
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public string? PaymentUrl { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, string> AdditionalData { get; set; } = new();
}

/// <summary>
/// Payment status result
/// </summary>
public class PaymentStatusResult
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public PaymentStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string? PaymentMethod { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, string> AdditionalData { get; set; } = new();
}

/// <summary>
/// Payment webhook result
/// </summary>
public class PaymentWebhookResult
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public PaymentStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string? PaymentMethod { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, string> AdditionalData { get; set; } = new();
}

/// <summary>
/// Payment refund result
/// </summary>
public class PaymentRefundResult
{
    public bool Success { get; set; }
    public string? RefundId { get; set; }
    public decimal RefundAmount { get; set; }
    public RefundStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, string> AdditionalData { get; set; } = new();
}

/// <summary>
/// Payment status enumeration
/// </summary>
public enum PaymentStatus
{
    Pending = 1,
    Processing = 2,
    Success = 3,
    Failed = 4,
    Cancelled = 5,
    Refunded = 6
}

/// <summary>
/// Refund status enumeration
/// </summary>
public enum RefundStatus
{
    Pending = 1,
    Processing = 2,
    Success = 3,
    Failed = 4
}

