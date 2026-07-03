using HomoeoDesk.Global.Domain.Common;
using HomoeoDesk.Global.Domain.Enums;

namespace HomoeoDesk.Global.Domain.Entities;

public class PaymentTransaction : BaseEntity
{
    public int TenantId { get; set; }
    public int SubscriptionId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentGateway { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public DateTime? PaymentDate { get; set; }

    public GlobalTenant GlobalTenant { get; set; } = null!;
    public OrganizationSubscription Subscription { get; set; } = null!;
}
