using ClinicCare.Domain.Enums;

namespace ClinicCare.Application.Features.PurchaseOrders.Queries.GetPurchaseOrders;

public class PurchaseOrderDto
{
    public int Id { get; set; }
    public int ClinicId { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public int Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public string? Notes { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public int? ApprovedByUserId { get; set; }
    public string? ApprovedByUserName { get; set; }
    public DateTime? OrderedDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public int? ReceivedByUserId { get; set; }
    public string? ReceivedByUserName { get; set; }
    public List<PurchaseOrderItemDto> Items { get; set; } = new();
}

public class PurchaseOrderItemDto
{
    public int Id { get; set; }
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int? ReceivedQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal TotalPrice { get; set; }
    public string? BatchNumber { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public string? Notes { get; set; }
}

