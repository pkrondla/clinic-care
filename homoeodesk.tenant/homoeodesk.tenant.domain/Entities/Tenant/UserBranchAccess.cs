using HomoeoDesk.Tenant.Domain.Common;

namespace HomoeoDesk.Tenant.Domain.Entities;

/// <summary>
/// User Clinic Access - Tenant Entity
/// Manages which users can access which Branches within an organization
/// </summary>
public class UserBranchAccess : BaseEntity
{
    public int UserId { get; set; }
    public int BranchId { get; set; }
    public bool CanAccess { get; set; } = true;
    
    // Navigation Properties
    public User User { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
}

