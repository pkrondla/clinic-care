using ClinicCare.Domain.Common;

namespace ClinicCare.Domain.Entities;

/// <summary>
/// System User - Global Entity
/// Super admins and system administrators
/// </summary>
public class SystemUser : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int Role { get; set; } // 1=SuperAdmin, 2=SystemAdmin
    public DateTime? LastLoginAt { get; set; }
    
    public string FullName => $"{FirstName} {LastName}";
}

