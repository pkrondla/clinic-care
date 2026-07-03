using HomoeoDesk.Tenant.Domain.Common;
using HomoeoDesk.Tenant.Domain.Enums;

namespace HomoeoDesk.Tenant.Domain.Entities;

public class PurchaseOrder : TenantEntity
{
    public int BranchId { get; set; }
    public int SupplierId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public PurchaseOrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public string? Notes { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public int? ApprovedByUserId { get; set; }
    public DateTime? OrderedDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public int? ReceivedByUserId { get; set; }

    // Navigation Properties
    public Branch Branch { get; set; } = null!;
    public Supplier Supplier { get; set; } = null!;
    public User? ApprovedByUser { get; set; }
    public User? ReceivedByUser { get; set; }
    public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
}

public class PurchaseOrderItem : TenantEntity
{
    public int PurchaseOrderId { get; set; }
    public int MedicineId { get; set; }
    public int Quantity { get; set; }
    public int? ReceivedQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal TotalPrice { get; set; }
    public string? BatchNumber { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public string? Notes { get; set; }

    // Navigation Properties
    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public ClinicMedicine Medicine { get; set; } = null!;
}

