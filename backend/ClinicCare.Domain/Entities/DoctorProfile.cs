using ClinicCare.Domain.Common;
using ClinicCare.Domain.Modules.Appointments.Entities;

namespace ClinicCare.Domain.Entities;

public class DoctorProfile : TenantEntity
{
    public int UserId { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Qualification { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public string Specialization { get; set; } = string.Empty;
    public decimal ConsultationFeeInPerson { get; set; }
    public decimal ConsultationFeeTele { get; set; }
    public decimal FollowupFeeInPerson { get; set; }
    public decimal FollowupFeeTele { get; set; }

    // Base clinic - the primary clinic where doctor works
    public int? BaseClinicId { get; set; }

    // Navigation Properties
    public User User { get; set; } = null!;
    public Organization Organization { get; set; } = null!;
    public Clinic? BaseClinic { get; set; }
    public ICollection<DoctorAvailability> Availabilities { get; set; } = new List<DoctorAvailability>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
}
