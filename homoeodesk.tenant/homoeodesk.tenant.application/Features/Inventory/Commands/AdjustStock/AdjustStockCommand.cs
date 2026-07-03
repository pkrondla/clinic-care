using HomoeoDesk.Tenant.Application.Common.Models;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace HomoeoDesk.Tenant.Application.Features.Inventory.Commands.AdjustStock;

public class AdjustStockCommand : IRequest<Result<InventoryDto>>
{
    public int InventoryId { get; set; }

    [Required]
    public int Quantity { get; set; } // Can be positive (add) or negative (subtract)

    [Required]
    [MaxLength(50)]
    public string TransactionType { get; set; } = string.Empty; // "Purchase", "Sale", "Adjustment", "Return", "Expired"

    [MaxLength(500)]
    public string? Notes { get; set; }
}

public class InventoryDto
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int MinimumStock { get; set; }
    public int MaximumStock { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    public DateOnly ExpiryDate { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public bool IsLowStock => CurrentStock <= MinimumStock;
    public DateTime LastUpdated { get; set; }
}

