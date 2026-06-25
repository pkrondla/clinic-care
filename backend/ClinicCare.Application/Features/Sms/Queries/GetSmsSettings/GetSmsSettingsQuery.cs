using MediatR;

namespace ClinicCare.Application.Features.Sms.Queries.GetSmsSettings;

public class GetSmsSettingsQuery : IRequest<SmsSettingsDto?>
{
}

public class SmsSettingsDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public bool IsEnabled { get; set; }
    public string? Provider { get; set; }
    public string? ApiKey { get; set; } // Decrypted for display
    public string? ApiSecret { get; set; } // Decrypted for display
    public string? AccountSid { get; set; }
    public string? AuthToken { get; set; } // Decrypted for display
    public string? FromPhoneNumber { get; set; }
    public string? SenderId { get; set; }
    public string? ApiUrl { get; set; }
    public int? TimeoutSeconds { get; set; }
}

