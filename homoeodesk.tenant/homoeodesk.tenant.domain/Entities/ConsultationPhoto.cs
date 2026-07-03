using HomoeoDesk.Tenant.Domain.Common;

namespace HomoeoDesk.Tenant.Domain.Entities;

public class ConsultationPhoto : TenantEntity
{
    public int ConsultationId { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }

    // Navigation Properties
    public Consultation Consultation { get; set; } = null!;
}

