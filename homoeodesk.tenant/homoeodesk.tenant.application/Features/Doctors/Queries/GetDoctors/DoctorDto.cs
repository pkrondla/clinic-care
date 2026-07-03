namespace HomoeoDesk.Tenant.Application.Features.Doctors.Queries.GetDoctors;

public class DoctorDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
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

