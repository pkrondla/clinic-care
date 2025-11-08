using ClinicCare.Domain.Common;
using ClinicCare.Domain.Enums;

namespace ClinicCare.Domain.Entities;

public class Inventory : TenantEntity
{
    public int ClinicId { get; set; }
    public int MedicineId { get; set; }
    public int CurrentStock { get; set; }
    public int MinimumStock { get; set; }
    public int MaximumStock { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    public DateOnly ExpiryDate { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }

    public bool IsLowStock => CurrentStock <= MinimumStock;
    public bool IsExpired => ExpiryDate <= DateOnly.FromDateTime(DateTime.Now);
    public bool IsNearExpiry => ExpiryDate <= DateOnly.FromDateTime(DateTime.Now.AddDays(30));

    // Navigation Properties
    public Organization Organization { get; set; } = null!;
    public Clinic Clinic { get; set; } = null!;
    public ClinicMedicine Medicine { get; set; } = null!;
}

public class StockTransaction : TenantEntity
{
    public int ClinicId { get; set; }
    public int MedicineId { get; set; }
    public TransactionType TransactionType { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public int? FromClinicId { get; set; }
    public int? ToClinicId { get; set; }
    public DateTime TransactionDate { get; set; }

    // Navigation Properties
    public Organization Organization { get; set; } = null!;
    public Clinic Clinic { get; set; } = null!;
    public ClinicMedicine Medicine { get; set; } = null!;
    public Clinic? FromClinic { get; set; }
    public Clinic? ToClinic { get; set; }
}
