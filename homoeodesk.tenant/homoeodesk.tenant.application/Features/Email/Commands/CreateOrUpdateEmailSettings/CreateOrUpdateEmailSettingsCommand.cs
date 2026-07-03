using HomoeoDesk.Tenant.Application.Features.Email.Queries.GetEmailSettings;
using MediatR;

namespace HomoeoDesk.Tenant.Application.Features.Email.Commands.CreateOrUpdateEmailSettings;

public class CreateOrUpdateEmailSettingsCommand : IRequest<EmailSettingsDto>
{
    public bool IsEnabled { get; set; }
    public string? SmtpServer { get; set; }
    public int? SmtpPort { get; set; }
    public bool UseSsl { get; set; } = true;
    public bool UseTls { get; set; } = true;
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; } // Will be encrypted
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
    public string? ReplyToEmail { get; set; }
    public int? TimeoutSeconds { get; set; }
}

