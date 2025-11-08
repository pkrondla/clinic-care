using ClinicCare.Domain.Common;

namespace ClinicCare.Domain.Entities;

public class GlobalMedicine : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string GenericName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Potency { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;

    // Navigation Properties
    public ICollection<ClinicMedicine> ClinicMedicines { get; set; } = new List<ClinicMedicine>();
}
