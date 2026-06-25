using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Domain.Enums;
using MediatR;

namespace ClinicCare.Application.Features.WhatsApp.Commands.CreateOrUpdateWhatsAppSettings;

public class CreateOrUpdateWhatsAppSettingsCommand : IRequest<WhatsAppSettingsDto>
{
    public bool IsEnabled { get; set; }
    public WhatsAppProvider Provider { get; set; } = WhatsAppProvider.Meta;
    
    // Meta WhatsApp Business API fields
    public string? PhoneNumberId { get; set; }
    public string? BusinessAccountId { get; set; }
    public string? AccessToken { get; set; } // Will be encrypted
    
    // Other provider fields (for future use)
    public string? ApiKey { get; set; } // Will be encrypted
    public string? ApiSecret { get; set; } // Will be encrypted
    
    // Webhook configuration
    public string? WebhookUrl { get; set; }
    public string? WebhookSecret { get; set; } // Will be encrypted
    public string? WebhookVerifyToken { get; set; }
    
    // Additional settings
    public string? ApiVersion { get; set; } // e.g., "v18.0" for Meta
    public string? FromPhoneNumber { get; set; } // WhatsApp number
}

