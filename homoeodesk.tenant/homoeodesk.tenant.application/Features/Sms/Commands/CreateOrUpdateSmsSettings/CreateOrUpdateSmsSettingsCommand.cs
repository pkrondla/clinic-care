using HomoeoDesk.Tenant.Application.Features.Sms.Queries.GetSmsSettings;
using MediatR;

namespace HomoeoDesk.Tenant.Application.Features.Sms.Commands.CreateOrUpdateSmsSettings;

public class CreateOrUpdateSmsSettingsCommand : IRequest<SmsSettingsDto>
{
    public bool IsEnabled { get; set; }
    public string? Provider { get; set; }
    public string? ApiKey { get; set; } // Will be encrypted
    public string? ApiSecret { get; set; } // Will be encrypted
    public string? AccountSid { get; set; }
    public string? AuthToken { get; set; } // Will be encrypted
    public string? FromPhoneNumber { get; set; }
    public string? SenderId { get; set; }
    public string? ApiUrl { get; set; }
    public int? TimeoutSeconds { get; set; }
}

