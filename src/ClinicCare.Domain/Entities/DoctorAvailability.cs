using ClinicCare.Domain.Common;

namespace ClinicCare.Domain.Entities;

public class DoctorAvailability : TenantEntity
{
    public int DoctorId { get; set; }
    public int ClinicId { get; set; }
    public DateOnly AvailableDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    // Navigation Properties
    public DoctorProfile Doctor { get; set; } = null!;
    public Clinic Clinic { get; set; } = null!;
    public Organization Organization { get; set; } = null!;
}
