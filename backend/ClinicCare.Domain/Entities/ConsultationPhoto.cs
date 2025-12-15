using ClinicCare.Domain.Common;

namespace ClinicCare.Domain.Entities;

public class ConsultationPhoto : TenantEntity
{
    public int ConsultationId { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }

    // Navigation Properties
    public Organization Organization { get; set; } = null!;
    public Consultation Consultation { get; set; } = null!;
}

