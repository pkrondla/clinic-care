namespace HomoeoDesk.Tenant.Infrastructure.Configuration;

public class TenantStampOptions
{
    public const string SectionName = "TenantStamp";

    public bool EnableFixedTenant { get; set; }
    public int FixedTenantId { get; set; } = 1;
    public string? FixedTenantConnectionString { get; set; }
    public string? FixedTenantSubdomain { get; set; } = "demo";

    /// <summary>When set and EnforceTrialExpiry is true, returns HTTP 402 after this date.</summary>
    public DateTime? TrialEndDate { get; set; }

    public bool EnforceTrialExpiry { get; set; }
}
