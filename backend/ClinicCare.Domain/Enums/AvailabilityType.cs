namespace ClinicCare.Domain.Enums;

/// <summary>
/// Defines the type of doctor availability entry
/// </summary>
public enum AvailabilityType
{
    /// <summary>
    /// Regular availability at base clinic (follows clinic operating hours)
    /// </summary>
    Regular = 0,

    /// <summary>
    /// Doctor is available at a different clinic for this date/time
    /// </summary>
    DifferentClinic = 1,

    /// <summary>
    /// Doctor is on leave (not available)
    /// </summary>
    Leave = 2,

    /// <summary>
    /// Modified hours (arriving late, leaving early, or custom hours)
    /// </summary>
    ModifiedHours = 3
}

