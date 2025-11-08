using ClinicCare.Domain.Common;
using ClinicCare.Domain.Enums;

namespace ClinicCare.Domain.Entities;

public class Communication : TenantEntity
{
    public int PatientId { get; set; }
    public CommunicationType Type { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string RecipientContact { get; set; } = string.Empty;
    public CommunicationStatus Status { get; set; }
    public string Reference { get; set; } = string.Empty;
    public DateTime? ScheduledAt { get; set; }
    public DateTime? SentAt { get; set; }

    // Navigation Properties
    public Organization Organization { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
}
