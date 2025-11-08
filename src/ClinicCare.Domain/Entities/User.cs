using ClinicCare.Domain.Common;
using ClinicCare.Domain.Enums;

namespace ClinicCare.Domain.Entities;

public class User : TenantEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public int? SelectedClinicId { get; set; }
    
    public string FullName => $"{FirstName} {LastName}";

    // Navigation Properties
    public Organization Organization { get; set; } = null!;
    public ICollection<UserOrganization> UserOrganizations { get; set; } = new List<UserOrganization>();
    public DoctorProfile? DoctorProfile { get; set; }
    public Patient? Patient { get; set; }
}
