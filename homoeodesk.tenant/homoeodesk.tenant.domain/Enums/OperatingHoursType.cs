namespace HomoeoDesk.Tenant.Domain.Enums;

/// <summary>
/// Defines the type of operating hours for a clinic
/// </summary>
public enum OperatingHoursType
{
    /// <summary>
    /// Single continuous shift (e.g., 10 AM - 5 PM)
    /// </summary>
    SingleShift = 0,

    /// <summary>
    /// Split shift with morning and evening sessions (e.g., 10 AM - 2 PM, 5 PM - 8 PM)
    /// </summary>
    SplitShift = 1
}

