namespace HomoeoDesk.Tenant.Application.Common.Services;

public interface IEmailService
{
    Task<EmailSendResult> SendEmailAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default);
    Task<EmailSendResult> SendEmailAsync(string to, string subject, string body, List<string>? cc, List<string>? bcc, List<EmailAttachment>? attachments, bool isHtml = true, CancellationToken cancellationToken = default);
    Task<EmailSendResult> SendTemplatedEmailAsync(string to, string templateName, Dictionary<string, string> templateData, CancellationToken cancellationToken = default);
}

public class EmailSendResult
{
    public bool Success { get; set; }
    public string? MessageId { get; set; }
    public string? ErrorMessage { get; set; }
}

public class EmailAttachment
{
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "application/octet-stream";
}

