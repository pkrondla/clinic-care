using ClinicCare.Domain.Common;

namespace ClinicCare.Domain.Entities;

public class ClinicMedicine : TenantEntity
{
    public int ClinicId { get; set; }
    public int? GlobalMedicineId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string GenericName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Potency { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    public string Description { get; set; } = string.Empty;

    // Navigation Properties
    public Organization Organization { get; set; } = null!;
    public Clinic Clinic { get; set; } = null!;
    public GlobalMedicine? GlobalMedicine { get; set; }
    public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    public ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
    // PrescriptionItems collection removed - we don't need to navigate from medicine to prescription items
    // This prevents EF Core from creating the ClinicMedicineId1 shadow property
}
