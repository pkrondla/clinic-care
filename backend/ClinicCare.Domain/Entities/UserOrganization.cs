using ClinicCare.Domain.Common;
using ClinicCare.Domain.Enums;

namespace ClinicCare.Domain.Entities;

public class UserOrganization : BaseEntity
{
    public int UserId { get; set; }
    public int OrganizationId { get; set; }
    public UserRole Role { get; set; }

    // Navigation Properties
    public User User { get; set; } = null!;
    public Organization Organization { get; set; } = null!;
}
