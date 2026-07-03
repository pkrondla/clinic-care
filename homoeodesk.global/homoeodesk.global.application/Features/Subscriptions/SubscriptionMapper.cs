using HomoeoDesk.Global.Domain.Entities;
using HomoeoDesk.Global.Domain.Enums;

namespace HomoeoDesk.Global.Application.Features.Subscriptions;

internal static class SubscriptionMapper
{
    public static SubscriptionDto ToDto(OrganizationSubscription subscription)
    {
        var plan = subscription.SubscriptionPlan;
        var tenant = subscription.GlobalTenant;

        return new SubscriptionDto
        {
            Id = subscription.Id,
            CreatedAt = subscription.CreatedAt,
            UpdatedAt = subscription.UpdatedAt,
            IsActive = subscription.IsActive,
            Organization = new SubscriptionOrganizationDto
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Subdomain = tenant.Subdomain,
                ContactEmail = tenant.ContactEmail,
                ContactPhone = string.IsNullOrWhiteSpace(tenant.ContactPhone) ? null : tenant.ContactPhone,
                Address = string.IsNullOrWhiteSpace(tenant.Address) ? null : tenant.Address
            },
            Plan = plan.Name,
            Status = MapStatus(subscription.Status),
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            Price = plan.Price,
            MaxClinics = plan.MaxClinics,
            MaxUsers = plan.MaxDoctors,
            Features = ParseFeatures(plan.Features),
            PaymentHistory = subscription.PaymentTransactions
                .OrderByDescending(p => p.PaymentDate ?? p.CreatedAt)
                .Select(MapPayment)
                .ToList()
        };
    }

    private static PaymentHistoryDto MapPayment(PaymentTransaction payment) => new()
    {
        Id = payment.Id,
        Date = payment.PaymentDate ?? payment.CreatedAt,
        Amount = payment.Amount,
        Status = payment.Status switch
        {
            PaymentStatus.Success => "Paid",
            PaymentStatus.Failed => "Failed",
            _ => "Pending"
        },
        Method = payment.PaymentMethod
    };

    private static string MapStatus(SubscriptionStatus status) => status switch
    {
        SubscriptionStatus.Trial => "Pending",
        SubscriptionStatus.Active => "Active",
        SubscriptionStatus.Expired => "Expired",
        SubscriptionStatus.Cancelled => "Cancelled",
        SubscriptionStatus.Suspended => "Inactive",
        _ => status.ToString()
    };

    private static string[] ParseFeatures(string features) =>
        string.IsNullOrWhiteSpace(features)
            ? Array.Empty<string>()
            : features.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    public static SubscriptionStatus MapStatusForFilter(string status) => status switch
    {
        "Pending" => SubscriptionStatus.Trial,
        "Active" => SubscriptionStatus.Active,
        "Expired" => SubscriptionStatus.Expired,
        "Cancelled" => SubscriptionStatus.Cancelled,
        "Inactive" => SubscriptionStatus.Suspended,
        _ => Enum.Parse<SubscriptionStatus>(status, ignoreCase: true)
    };
}
