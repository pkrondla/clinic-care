using HomoeoDesk.Tenant.Domain.Enums;

namespace HomoeoDesk.Tenant.Application.Features.Users.Queries.GetUsers;

public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public int? SelectedBranchId { get; set; }
    public string? SelectedBranchName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public List<BranchAccessDto> ClinicAccess { get; set; } = new();
    public DoctorProfileDto? DoctorProfile { get; set; }
}

public class BranchAccessDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string BranchCode { get; set; } = string.Empty;
    public bool CanAccess { get; set; }
}

public class DoctorProfileDto
{
    public int Id { get; set; }
    public string Qualification { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public decimal ConsultationFeeInPerson { get; set; }
    public decimal ConsultationFeeTele { get; set; }
    public decimal FollowupFeeInPerson { get; set; }
    public decimal FollowupFeeTele { get; set; }
    public bool IsActive { get; set; }
}

