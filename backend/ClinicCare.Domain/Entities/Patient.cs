using ClinicCare.Domain.Common;

namespace ClinicCare.Domain.Entities;

public class Patient : TenantEntity
{
    public int UserId { get; set; }
    public string PatientCode { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string BloodGroup { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string EmergencyContact { get; set; } = string.Empty;
    public string MedicalHistory { get; set; } = string.Empty;

    public int Age => DateTime.Now.Year - DateOfBirth.Year;

    // Navigation Properties
    public User User { get; set; } = null!;
    public Organization Organization { get; set; } = null!;
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<Communication> Communications { get; set; } = new List<Communication>();
}
