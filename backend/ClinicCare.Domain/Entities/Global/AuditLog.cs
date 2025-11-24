using ClinicCare.Domain.Common;

namespace ClinicCare.Domain.Entities;

/// <summary>
/// Audit Log - Global Entity
/// System-wide audit trail for tracking changes
/// </summary>
public class AuditLog
{
    public long Id { get; set; }
    public int? OrganizationId { get; set; } // NULL for system-level actions
    public int? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string Action { get; set; } = string.Empty; // Created, Updated, Deleted, Login, etc.
    public string EntityType { get; set; } = string.Empty; // Organization, User, Medicine, etc.
    public int? EntityId { get; set; }
    public string? Details { get; set; } // JSON with changes
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

