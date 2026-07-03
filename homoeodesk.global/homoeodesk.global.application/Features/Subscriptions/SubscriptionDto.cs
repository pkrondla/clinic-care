namespace HomoeoDesk.Global.Application.Features.Subscriptions;

public class SubscriptionDto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    public SubscriptionOrganizationDto Organization { get; set; } = null!;
    public string Plan { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Price { get; set; }
    public int MaxClinics { get; set; }
    public int MaxUsers { get; set; }
    public string[] Features { get; set; } = Array.Empty<string>();
    public List<PaymentHistoryDto> PaymentHistory { get; set; } = new();
}

public class SubscriptionOrganizationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
}

public class PaymentHistoryDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
}
