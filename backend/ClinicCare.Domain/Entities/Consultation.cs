using ClinicCare.Domain.Common;
using ClinicCare.Domain.Modules.Appointments.Entities;

namespace ClinicCare.Domain.Entities;

public class Consultation : TenantEntity
{
    public int AppointmentId { get; set; }
    public int DoctorId { get; set; }
    public int PatientId { get; set; }
    public string ChiefComplaint { get; set; } = string.Empty;
    public string Symptoms { get; set; } = string.Empty;
    public string Examination { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string TreatmentPlan { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public decimal ConsultationFee { get; set; }
    public DateTime ConsultationDate { get; set; }
    public int ConsultationType { get; set; } // 1 = InPerson, 2 = Teleconsultation (matches AppointmentType enum)

    // Navigation Properties
    public Organization Organization { get; set; } = null!;
    public Appointment Appointment { get; set; } = null!;
    public DoctorProfile Doctor { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    public Invoice? Invoice { get; set; }
}
