using ClinicCare.Domain.Common;

namespace ClinicCare.Domain.Entities;

public class Clinic : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;

    // Navigation Properties
    public Organization Organization { get; set; } = null!;
    public ICollection<DoctorAvailability> DoctorAvailabilities { get; set; } = new List<DoctorAvailability>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<ClinicMedicine> ClinicMedicines { get; set; } = new List<ClinicMedicine>();
    public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
