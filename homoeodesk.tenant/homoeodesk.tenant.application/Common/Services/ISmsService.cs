namespace HomoeoDesk.Tenant.Application.Common.Services;

public interface ISmsService
{
    Task<SmsSendResult> SendSmsAsync(string to, string message, CancellationToken cancellationToken = default);
    Task<SmsMessageStatus> GetMessageStatusAsync(string messageId, CancellationToken cancellationToken = default);
}

public class SmsSendResult
{
    public bool Success { get; set; }
    public string? MessageId { get; set; }
    public string? ErrorMessage { get; set; }
}

public enum SmsMessageStatus
{
    Pending = 1,
    Sent = 2,
    Delivered = 3,
    Failed = 4
}

