using HomoeoDesk.Tenant.Domain.Common;

namespace HomoeoDesk.Tenant.Domain.Entities;

public class SmsSettings : TenantEntity
{
    public bool IsEnabled { get; set; } = false;
    
    // Provider Configuration
    public string? Provider { get; set; } // e.g., "Twilio", "AWS SNS", "Vonage", etc.
    
    // API Credentials (encrypted)
    public string? ApiKey { get; set; } // Encrypted
    public string? ApiSecret { get; set; } // Encrypted
    public string? AccountSid { get; set; } // For Twilio
    public string? AuthToken { get; set; } // Encrypted
    
    // Sender Information
    public string? FromPhoneNumber { get; set; }
    public string? SenderId { get; set; } // For some providers
    
    // Additional settings
    public string? ApiUrl { get; set; }
    public int? TimeoutSeconds { get; set; } = 30;
    
    // Navigation Properties
}

