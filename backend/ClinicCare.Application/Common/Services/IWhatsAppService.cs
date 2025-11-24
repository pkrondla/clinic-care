namespace ClinicCare.Application.Common.Services;

public interface IWhatsAppService
{
    Task<WhatsAppSendResult> SendTextMessageAsync(string to, string message, CancellationToken cancellationToken = default);
    Task<WhatsAppSendResult> SendMediaMessageAsync(string to, byte[] media, string mediaType, string? caption = null, CancellationToken cancellationToken = default);
    Task<WhatsAppMessageStatus> GetMessageStatusAsync(string messageId, CancellationToken cancellationToken = default);
}

public class WhatsAppSendResult
{
    public bool Success { get; set; }
    public string? MessageId { get; set; }
    public string? ErrorMessage { get; set; }
}

public enum WhatsAppMessageStatus
{
    Pending = 1,
    Sent = 2,
    Delivered = 3,
    Read = 4,
    Failed = 5
}

