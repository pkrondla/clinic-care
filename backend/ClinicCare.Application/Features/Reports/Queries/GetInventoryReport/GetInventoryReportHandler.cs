using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Models;
using ClinicCare.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.Reports.Queries.GetInventoryReport;

public class GetInventoryReportHandler : IRequestHandler<GetInventoryReportQuery, Result<InventoryReportDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetInventoryReportHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<InventoryReportDto>> Handle(GetInventoryReportQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
            {
                return Result<InventoryReportDto>.Failure("User not associated with any organization");
            }

            // Get all clinics in organization
            var clinicsQuery = _context.Clinics
                .Where(c => c.OrganizationId == organizationId.Value && c.IsActive);

            if (request.ClinicId.HasValue)
            {
                clinicsQuery = clinicsQuery.Where(c => c.Id == request.ClinicId.Value);
            }

            var clinics = await clinicsQuery.ToListAsync(cancellationToken);
            var clinicIds = clinics.Select(c => c.Id).ToList();

            // Get all inventory items
            var inventoryQuery = _context.Inventories
                .Include(i => i.Medicine)
                .Include(i => i.Clinic)
                .Where(i => i.OrganizationId == organizationId.Value 
                    && i.IsActive 
                    && clinicIds.Contains(i.ClinicId));

            if (request.MedicineId.HasValue)
            {
                inventoryQuery = inventoryQuery.Where(i => i.MedicineId == request.MedicineId.Value);
            }

            if (request.LowStockOnly == true)
            {
                inventoryQuery = inventoryQuery.Where(i => i.CurrentStock <= i.MinimumStock);
            }

            var inventories = await inventoryQuery.ToListAsync(cancellationToken);

            // Get stock transactions for movement summary
            var transactionsQuery = _context.StockTransactions
                .Include(t => t.Medicine)
                .Include(t => t.Clinic)
                .Where(t => t.OrganizationId == organizationId.Value 
                    && t.IsActive
                    && clinicIds.Contains(t.ClinicId));

            if (request.ClinicId.HasValue)
            {
                transactionsQuery = transactionsQuery.Where(t => t.ClinicId == request.ClinicId.Value);
            }

            if (request.MedicineId.HasValue)
            {
                transactionsQuery = transactionsQuery.Where(t => t.MedicineId == request.MedicineId.Value);
            }

            // Get recent transactions (last 30 days)
            var recentDate = DateTime.UtcNow.AddDays(-30);
            var recentTransactions = await transactionsQuery
                .Where(t => t.TransactionDate >= recentDate)
                .OrderByDescending(t => t.TransactionDate)
                .Take(100)
                .ToListAsync(cancellationToken);

            // Build combined inventory (grouped by medicine across all clinics)
            var combinedInventory = inventories
                .GroupBy(i => i.MedicineId)
                .Select(g => {
                    var firstItem = g.First();
                    var clinicStocks = g.Select(i => new ClinicStockDto
                    {
                        ClinicId = i.ClinicId,
                        ClinicName = i.Clinic.Name,
                        Quantity = i.CurrentStock,
                        AvailableQuantity = i.CurrentStock,
                        ReservedQuantity = 0, // Not tracked in current model
                        UnitPrice = i.SellingPrice,
                        TotalValue = i.CurrentStock * i.SellingPrice,
                        ReorderLevel = i.MinimumStock,
                        IsLowStock = i.CurrentStock <= i.MinimumStock
                    }).ToList();

                    var totalQuantity = g.Sum(i => i.CurrentStock);
                    var totalAvailable = g.Sum(i => i.CurrentStock);
                    var totalReserved = 0; // Not tracked in current model
                    var totalValue = g.Sum(i => i.CurrentStock * i.SellingPrice);
                    var avgPrice = g.Average(i => i.SellingPrice);

                    return new CombinedInventoryItemDto
                    {
                        MedicineId = firstItem.MedicineId,
                        MedicineName = firstItem.Medicine?.Name ?? string.Empty,
                        MedicineCode = firstItem.Medicine?.Id.ToString() ?? string.Empty, // Using ID as code since Code field doesn't exist
                        Unit = "units", // Unit not stored in ClinicMedicine, using default
                        TotalQuantity = totalQuantity,
                        AvailableQuantity = totalAvailable,
                        ReservedQuantity = totalReserved,
                        AveragePrice = avgPrice,
                        TotalValue = totalValue,
                        ClinicCount = g.Count(),
                        ClinicStocks = clinicStocks
                    };
                })
                .OrderBy(i => i.MedicineName)
                .ToList();

            // Build clinic inventory
            var clinicInventory = clinics.Select(clinic => {
                var clinicStocks = inventories
                    .Where(i => i.ClinicId == clinic.Id)
                    .Select(i => new ClinicStockDto
                    {
                        ClinicId = i.ClinicId,
                        ClinicName = i.Clinic.Name,
                        Quantity = i.CurrentStock,
                        AvailableQuantity = i.CurrentStock,
                        ReservedQuantity = 0, // Not tracked in current model
                        UnitPrice = i.SellingPrice,
                        TotalValue = i.CurrentStock * i.SellingPrice,
                        ReorderLevel = i.MinimumStock,
                        IsLowStock = i.CurrentStock <= i.MinimumStock
                    }).ToList();

                return new ClinicInventoryDto
                {
                    ClinicId = clinic.Id,
                    ClinicName = clinic.Name,
                    MedicineCount = clinicStocks.Count,
                    TotalValue = clinicStocks.Sum(s => s.TotalValue),
                    LowStockCount = clinicStocks.Count(s => s.IsLowStock && s.AvailableQuantity > 0),
                    OutOfStockCount = clinicStocks.Count(s => s.AvailableQuantity <= 0),
                    Stocks = clinicStocks.OrderBy(s => s.IsLowStock ? 0 : 1).ThenBy(s => s.ClinicName).ToList()
                };
            }).ToList();

            // Build low stock alerts
            var lowStockAlerts = inventories
                .Where(i => i.CurrentStock <= i.MinimumStock)
                .Select(i => new LowStockAlertDto
                {
                    ClinicId = i.ClinicId,
                    ClinicName = i.Clinic.Name,
                    MedicineId = i.MedicineId,
                    MedicineName = i.Medicine?.Name ?? string.Empty,
                    MedicineCode = i.Medicine?.Id.ToString() ?? string.Empty, // Using ID as code since Code field doesn't exist
                    CurrentStock = i.CurrentStock,
                    ReorderLevel = i.MinimumStock,
                    RequiredQuantity = Math.Max(0, i.MinimumStock - i.CurrentStock + (i.MinimumStock * 2)), // Suggest 2x minimum stock
                    Unit = "units" // Unit not stored in ClinicMedicine, using default
                })
                .OrderBy(a => a.CurrentStock)
                .ThenBy(a => a.ClinicName)
                .ToList();

            // Build stock movement summary
            var stockMovements = recentTransactions.Select(t => new StockMovementDto
            {
                Date = t.TransactionDate,
                TransactionType = t.TransactionType.ToString(),
                ClinicName = t.Clinic?.Name ?? string.Empty,
                MedicineName = t.Medicine?.Name ?? string.Empty,
                Quantity = t.Quantity,
                Unit = "units", // Unit not stored in ClinicMedicine, using default
                UnitPrice = t.UnitPrice,
                TotalValue = t.Quantity * t.UnitPrice,
                Reference = t.Reference ?? string.Empty
            }).ToList();

            // Calculate totals
            var totalInventoryValue = inventories.Sum(i => i.CurrentStock * i.SellingPrice);
            var totalMedicines = combinedInventory.Count;
            var lowStockItems = lowStockAlerts.Count(a => a.CurrentStock > 0);
            var outOfStockItems = lowStockAlerts.Count(a => a.CurrentStock <= 0);

            var report = new InventoryReportDto
            {
                GeneratedAt = DateTime.UtcNow,
                TotalInventoryValue = totalInventoryValue,
                TotalMedicines = totalMedicines,
                LowStockItems = lowStockItems,
                OutOfStockItems = outOfStockItems,
                CombinedInventory = combinedInventory,
                ClinicInventory = clinicInventory,
                LowStockAlerts = lowStockAlerts,
                StockMovements = stockMovements
            };

            return Result<InventoryReportDto>.Success(report);
        }
        catch (Exception ex)
        {
            return Result<InventoryReportDto>.Failure($"Failed to generate inventory report: {ex.Message}");
        }
    }
}

