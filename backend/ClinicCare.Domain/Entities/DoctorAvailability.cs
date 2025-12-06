using ClinicCare.Domain.Common;
using ClinicCare.Domain.Enums;

namespace ClinicCare.Domain.Entities;

public class DoctorAvailability : TenantEntity
{
    public int DoctorId { get; set; }
    public int ClinicId { get; set; }
    public DateOnly AvailableDate { get; set; }
    public DateOnly? EndDate { get; set; } // For leave date ranges
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public AvailabilityType AvailabilityType { get; set; }
    public string? Notes { get; set; }

    // Navigation Properties
    public DoctorProfile Doctor { get; set; } = null!;
    public Clinic Clinic { get; set; } = null!;
    public Organization Organization { get; set; } = null!;
}
