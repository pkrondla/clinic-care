using ClinicCare.Domain.Common;
using ClinicCare.Domain.Enums;

namespace ClinicCare.Domain.Entities;

public class Organization : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public SubscriptionStatus SubscriptionStatus { get; set; } = SubscriptionStatus.Trial;
    public DateTime? TrialEndDate { get; set; }

    // Navigation Properties
    public ICollection<Clinic> Clinics { get; set; } = new List<Clinic>();
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<UserOrganization> UserOrganizations { get; set; } = new List<UserOrganization>();
    public ICollection<OrganizationSubscription> Subscriptions { get; set; } = new List<OrganizationSubscription>();
    public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
}
