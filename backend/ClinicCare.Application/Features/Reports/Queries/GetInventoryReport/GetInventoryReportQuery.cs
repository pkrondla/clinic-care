using ClinicCare.Application.Common.Models;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace ClinicCare.Application.Features.Reports.Queries.GetInventoryReport;

public class GetInventoryReportQuery : IRequest<Result<InventoryReportDto>>
{
    public int? ClinicId { get; set; }
    public int? MedicineId { get; set; }
    public bool? LowStockOnly { get; set; }
    public string? GroupBy { get; set; } // "clinic", "medicine", "category"
}

public class InventoryReportDto
{
    public DateTime GeneratedAt { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public int TotalMedicines { get; set; }
    public int LowStockItems { get; set; }
    public int OutOfStockItems { get; set; }
    
    // Combined inventory across all clinics
    public List<CombinedInventoryItemDto> CombinedInventory { get; set; } = new();
    
    // Inventory by clinic
    public List<ClinicInventoryDto> ClinicInventory { get; set; } = new();
    
    // Low stock alerts
    public List<LowStockAlertDto> LowStockAlerts { get; set; } = new();
    
    // Stock movement summary
    public List<StockMovementDto> StockMovements { get; set; } = new();
}

public class CombinedInventoryItemDto
{
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string MedicineCode { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal TotalValue { get; set; }
    public int ClinicCount { get; set; } // Number of clinics that have this medicine
    public List<ClinicStockDto> ClinicStocks { get; set; } = new();
}

public class ClinicStockDto
{
    public int ClinicId { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int AvailableQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalValue { get; set; }
    public int ReorderLevel { get; set; }
    public bool IsLowStock { get; set; }
}

public class ClinicInventoryDto
{
    public int ClinicId { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public int MedicineCount { get; set; }
    public decimal TotalValue { get; set; }
    public int LowStockCount { get; set; }
    public int OutOfStockCount { get; set; }
    public List<ClinicStockDto> Stocks { get; set; } = new();
}

public class LowStockAlertDto
{
    public int ClinicId { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string MedicineCode { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int ReorderLevel { get; set; }
    public int RequiredQuantity { get; set; }
    public string Unit { get; set; } = string.Empty;
}

public class StockMovementDto
{
    public DateTime Date { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string ClinicName { get; set; } = string.Empty;
    public string MedicineName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal TotalValue { get; set; }
    public string Reference { get; set; } = string.Empty;
}

