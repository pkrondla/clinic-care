using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Domain.Enums;
using MediatR;

namespace ClinicCare.Application.Features.WhatsApp.Queries.GetWhatsAppSettings;

public class GetWhatsAppSettingsQuery : IRequest<WhatsAppSettingsDto?>
{
}

public class WhatsAppSettingsDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public bool IsEnabled { get; set; }
    public WhatsAppProvider Provider { get; set; }
    public string? PhoneNumberId { get; set; }
    public string? BusinessAccountId { get; set; }
    public string? ApiVersion { get; set; }
    public string? FromPhoneNumber { get; set; }
    public string? WebhookUrl { get; set; }
    public string? WebhookVerifyToken { get; set; }
    
    // Note: Sensitive fields (AccessToken, ApiKey, ApiSecret, WebhookSecret) are not included in DTO
    // They should never be sent to the frontend
}

