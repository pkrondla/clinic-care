using HomoeoDesk.Global.Domain.Common;

namespace HomoeoDesk.Global.Domain.Entities;

public class SubscriptionPlan : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int BillingCycle { get; set; }
    public int MaxClinics { get; set; }
    public int MaxDoctors { get; set; }
    public int MaxPatients { get; set; }
    public string Features { get; set; } = string.Empty;

    public ICollection<OrganizationSubscription> OrganizationSubscriptions { get; set; } = new List<OrganizationSubscription>();
}
