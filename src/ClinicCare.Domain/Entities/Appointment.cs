using ClinicCare.Domain.Common;
using ClinicCare.Domain.Enums;

namespace ClinicCare.Domain.Entities;

public class Appointment : TenantEntity
{
    public int ClinicId { get; set; }
    public int DoctorId { get; set; }
    public int PatientId { get; set; }
    public DateOnly AppointmentDate { get; set; }
    public int TokenNumber { get; set; }
    public AppointmentType Type { get; set; }
    public AppointmentStatus Status { get; set; }
    public string Notes { get; set; } = string.Empty;

    // Navigation Properties
    public Organization Organization { get; set; } = null!;
    public Clinic Clinic { get; set; } = null!;
    public DoctorProfile Doctor { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public Consultation? Consultation { get; set; }
}
