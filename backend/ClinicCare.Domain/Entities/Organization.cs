using ClinicCare.Domain.Common;

namespace ClinicCare.Domain.Entities;

public class Organization : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    // Navigation Properties
    public ICollection<Clinic> Clinics { get; set; } = new List<Clinic>();
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<UserOrganization> UserOrganizations { get; set; } = new List<UserOrganization>();
}
