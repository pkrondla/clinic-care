using ClinicCare.Domain.Common;

namespace ClinicCare.Domain.Entities;

/// <summary>
/// User Clinic Access - Tenant Entity
/// Manages which users can access which clinics within an organization
/// </summary>
public class UserClinicAccess : BaseEntity
{
    public int UserId { get; set; }
    public int ClinicId { get; set; }
    public bool CanAccess { get; set; } = true;
    
    // Navigation Properties
    public User User { get; set; } = null!;
    public Clinic Clinic { get; set; } = null!;
}

