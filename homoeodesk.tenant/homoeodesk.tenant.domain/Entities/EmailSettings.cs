using HomoeoDesk.Tenant.Domain.Common;

namespace HomoeoDesk.Tenant.Domain.Entities;

public class EmailSettings : TenantEntity
{
    public bool IsEnabled { get; set; } = false;
    
    // SMTP Configuration
    public string? SmtpServer { get; set; }
    public int? SmtpPort { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public bool UseTls { get; set; } = true;
    
    // Authentication
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; } // Encrypted
    
    // Sender Information
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
    
    // Additional settings
    public string? ReplyToEmail { get; set; }
    public int? TimeoutSeconds { get; set; } = 30;
    
    // Navigation Properties
}

