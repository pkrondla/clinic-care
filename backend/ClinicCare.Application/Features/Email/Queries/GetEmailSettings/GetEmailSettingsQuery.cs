using MediatR;

namespace ClinicCare.Application.Features.Email.Queries.GetEmailSettings;

public class GetEmailSettingsQuery : IRequest<EmailSettingsDto?>
{
}

public class EmailSettingsDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public bool IsEnabled { get; set; }
    public string? SmtpServer { get; set; }
    public int? SmtpPort { get; set; }
    public bool UseSsl { get; set; }
    public bool UseTls { get; set; }
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; } // Decrypted for display
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
    public string? ReplyToEmail { get; set; }
    public int? TimeoutSeconds { get; set; }
}

