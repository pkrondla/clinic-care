using HomoeoDesk.Tenant.Domain.Common;
using HomoeoDesk.Tenant.Domain.Enums;
using HomoeoDesk.Tenant.Domain.Modules.Appointments.Entities;

namespace HomoeoDesk.Tenant.Domain.Entities;

public class Branch : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;

    // Operating Hours
    public OperatingHoursType OperatingHoursType { get; set; }
    public TimeSpan? MorningStartTime { get; set; }
    public TimeSpan? MorningEndTime { get; set; }
    public TimeSpan? EveningStartTime { get; set; }
    public TimeSpan? EveningEndTime { get; set; }
    public TimeSpan? FullDayStartTime { get; set; }
    public TimeSpan? FullDayEndTime { get; set; }

    // Navigation Properties
    public ICollection<DoctorAvailability> DoctorAvailabilities { get; set; } = new List<DoctorAvailability>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<ClinicMedicine> ClinicMedicines { get; set; } = new List<ClinicMedicine>();
    public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
