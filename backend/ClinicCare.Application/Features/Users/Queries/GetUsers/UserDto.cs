using ClinicCare.Domain.Enums;

namespace ClinicCare.Application.Features.Users.Queries.GetUsers;

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
    public int? SelectedClinicId { get; set; }
    public string? SelectedClinicName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public List<ClinicAccessDto> ClinicAccess { get; set; } = new();
    public DoctorProfileDto? DoctorProfile { get; set; }
}

public class ClinicAccessDto
{
    public int ClinicId { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public string ClinicCode { get; set; } = string.Empty;
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

