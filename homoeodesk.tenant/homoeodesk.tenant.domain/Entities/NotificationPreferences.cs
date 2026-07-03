using HomoeoDesk.Tenant.Domain.Common;
using HomoeoDesk.Tenant.Domain.Enums;

namespace HomoeoDesk.Tenant.Domain.Entities;

public class NotificationPreferences : TenantEntity
{
    public NotificationType NotificationType { get; set; }
    
    // Channel preferences
    public bool EnableWhatsApp { get; set; } = true;
    public bool EnableEmail { get; set; } = true;
    public bool EnableSMS { get; set; } = false;
    
    // Custom template (optional - if null, use default template)
    public string? Template { get; set; }
    
    // Note: IsActive is inherited from BaseEntity
    
    // Navigation Properties
}

