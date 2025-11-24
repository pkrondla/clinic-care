using ClinicCare.Domain.Common;
using ClinicCare.Domain.Enums;

namespace ClinicCare.Domain.Entities;

/// <summary>
/// Organization Subscription - Global Entity
/// Tracks active subscriptions for organizations
/// </summary>
public class OrganizationSubscription : BaseEntity
{
    public int OrganizationId { get; set; }
    public int SubscriptionPlanId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public SubscriptionStatus Status { get; set; } // Active, Expired, Cancelled
    public bool AutoRenew { get; set; }
    
    // Navigation Properties
    public Organization Organization { get; set; } = null!;
    public SubscriptionPlan SubscriptionPlan { get; set; } = null!;
    public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
}

