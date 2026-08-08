using HomoeoDesk.Global.Domain.Common;
using HomoeoDesk.Global.Domain.Enums;

namespace HomoeoDesk.Global.Domain.Entities;

public class TrialRequest : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? PatientsPerWeek { get; set; }
    public string? Message { get; set; }
    public string Source { get; set; } = "website";
    public TrialRequestStatus Status { get; set; } = TrialRequestStatus.New;
}
