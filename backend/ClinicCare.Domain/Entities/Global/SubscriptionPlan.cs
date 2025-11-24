using ClinicCare.Domain.Common;

namespace ClinicCare.Domain.Entities;

/// <summary>
/// Subscription Plan - Global Entity
/// Defines available subscription tiers for organizations
/// </summary>
public class SubscriptionPlan : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int BillingCycle { get; set; } // 1=Monthly, 2=Quarterly, 3=Yearly
    public int MaxClinics { get; set; }
    public int MaxDoctors { get; set; }
    public int MaxPatients { get; set; }
    public string Features { get; set; } = string.Empty; // JSON array of features
    
    // Navigation Properties
    public ICollection<OrganizationSubscription> OrganizationSubscriptions { get; set; } = new List<OrganizationSubscription>();
}

