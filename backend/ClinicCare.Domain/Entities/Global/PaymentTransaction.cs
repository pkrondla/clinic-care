using ClinicCare.Domain.Common;
using ClinicCare.Domain.Enums;

namespace ClinicCare.Domain.Entities;

/// <summary>
/// Payment Transaction - Global Entity
/// Tracks subscription payments for organizations
/// </summary>
public class PaymentTransaction : BaseEntity
{
    public int OrganizationId { get; set; }
    public int SubscriptionId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
    public string PaymentMethod { get; set; } = string.Empty; // Card, UPI, NetBanking, etc.
    public string PaymentGateway { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; } // Pending, Success, Failed, Refunded
    public DateTime? PaymentDate { get; set; }
    
    // Navigation Properties
    public Organization Organization { get; set; } = null!;
    public OrganizationSubscription Subscription { get; set; } = null!;
}

