using HomoeoDesk.Global.Domain.Common;
using HomoeoDesk.Global.Domain.Enums;

namespace HomoeoDesk.Global.Domain.Entities;

public class OrganizationSubscription : BaseEntity
{
    public int TenantId { get; set; }
    public int SubscriptionPlanId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public SubscriptionStatus Status { get; set; }
    public bool AutoRenew { get; set; }

    public GlobalTenant GlobalTenant { get; set; } = null!;
    public SubscriptionPlan SubscriptionPlan { get; set; } = null!;
    public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
}
