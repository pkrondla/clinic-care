using HomoeoDesk.Tenant.Domain.Common;
using HomoeoDesk.Tenant.Domain.Enums;

namespace HomoeoDesk.Tenant.Domain.Entities;

public class WhatsAppBusinessSettings : TenantEntity
{
    public bool IsEnabled { get; set; } = false;
    public WhatsAppProvider Provider { get; set; } = WhatsAppProvider.Meta;
    
    // Meta WhatsApp Business API fields
    public string? PhoneNumberId { get; set; }
    public string? BusinessAccountId { get; set; }
    public string? AccessToken { get; set; } // Encrypted
    public string? ApiKey { get; set; } // Encrypted (for other providers)
    public string? ApiSecret { get; set; } // Encrypted (for other providers)
    
    // Webhook configuration
    public string? WebhookUrl { get; set; }
    public string? WebhookSecret { get; set; } // Encrypted
    public string? WebhookVerifyToken { get; set; }
    
    // Additional settings
    public string? ApiVersion { get; set; } // e.g., "v18.0" for Meta
    public string? FromPhoneNumber { get; set; } // WhatsApp number
    
    // Navigation Properties
}

