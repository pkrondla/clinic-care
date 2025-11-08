using ClinicCare.Domain.Common;

namespace ClinicCare.Domain.Global;

public class Organization : BaseEntity
{
    public string Name { get; private set; }
    public string Subdomain { get; private set; }
    public string DatabaseName { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime SubscriptionStartDate { get; private set; }
    public DateTime SubscriptionEndDate { get; private set; }
    public int SubscriptionPlanId { get; private set; }
    
    private Organization() { } // For EF Core

    public static Organization Create(
        string name, 
        string subdomain, 
        int subscriptionPlanId,
        DateTime subscriptionStartDate,
        DateTime subscriptionEndDate)
    {
        return new Organization
        {
            Name = name,
            Subdomain = subdomain.ToLowerInvariant(),
            DatabaseName = $"ClinicCare_{subdomain.ToLowerInvariant()}",
            IsActive = true,
            SubscriptionPlanId = subscriptionPlanId,
            SubscriptionStartDate = subscriptionStartDate,
            SubscriptionEndDate = subscriptionEndDate,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateSubscription(
        int subscriptionPlanId,
        DateTime startDate,
        DateTime endDate)
    {
        SubscriptionPlanId = subscriptionPlanId;
        SubscriptionStartDate = startDate;
        SubscriptionEndDate = endDate;
        UpdatedAt = DateTime.UtcNow;
    }
}