using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Inventory.Queries.GetInventory;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace HomoeoDesk.Tenant.Application.Features.Inventory.Commands.PerformStockAudit;

public class PerformStockAuditCommand : IRequest<Result<StockAuditResultDto>>
{
    [Required]
    public int BranchId { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one audit item is required")]
    public List<StockAuditItemCommand> AuditItems { get; set; } = new();

    [MaxLength(500)]
    public string? Notes { get; set; }
}

public class StockAuditItemCommand
{
    [Required]
    public int InventoryId { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Physical stock must be greater than or equal to 0")]
    public int PhysicalStock { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}

public class StockAuditResultDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public DateTime AuditDate { get; set; }
    public int TotalItemsAudited { get; set; }
    public int ItemsWithVariance { get; set; }
    public List<StockAuditItemResultDto> Items { get; set; } = new();
    public string? Notes { get; set; }
}

public class StockAuditItemResultDto
{
    public int InventoryId { get; set; }
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public int SystemStock { get; set; }
    public int PhysicalStock { get; set; }
    public int Variance { get; set; }
    public bool StockAdjusted { get; set; }
    public string? Notes { get; set; }
}

