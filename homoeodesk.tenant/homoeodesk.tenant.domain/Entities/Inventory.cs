using HomoeoDesk.Tenant.Domain.Common;
using HomoeoDesk.Tenant.Domain.Enums;

namespace HomoeoDesk.Tenant.Domain.Entities;

public class Inventory : TenantEntity
{
    public int BranchId { get; set; }
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
    public Branch Branch { get; set; } = null!;
    public ClinicMedicine Medicine { get; set; } = null!;
}

public class StockTransaction : TenantEntity
{
    public int BranchId { get; set; }
    public int MedicineId { get; set; }
    public TransactionType TransactionType { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public int? FromBranchId { get; set; }
    public int? ToBranchId { get; set; }
    public DateTime TransactionDate { get; set; }

    // Navigation Properties
    public Branch Branch { get; set; } = null!;
    public ClinicMedicine Medicine { get; set; } = null!;
    public Branch? FromBranch { get; set; }
    public Branch? ToBranch { get; set; }
}
