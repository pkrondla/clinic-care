using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Reports.Queries.GetInventoryReport;

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

            // Get all Branches in organization
            var BranchesQuery = _context.Branches
                .Where(c => c.OrganizationId == organizationId.Value && c.IsActive);

            if (request.BranchId.HasValue)
            {
                BranchesQuery = BranchesQuery.Where(c => c.Id == request.BranchId.Value);
            }

            var Branches = await BranchesQuery.ToListAsync(cancellationToken);
            var BranchIds = Branches.Select(c => c.Id).ToList();

            // Get all inventory items
            var inventoryQuery = _context.Inventories
                .Include(i => i.Medicine)
                .Include(i => i.Branch)
                .Where(i => i.OrganizationId == organizationId.Value 
                    && i.IsActive 
                    && BranchIds.Contains(i.BranchId));

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
                .Include(t => t.Branch)
                .Where(t => t.OrganizationId == organizationId.Value 
                    && t.IsActive
                    && BranchIds.Contains(t.BranchId));

            if (request.BranchId.HasValue)
            {
                transactionsQuery = transactionsQuery.Where(t => t.BranchId == request.BranchId.Value);
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

            // Build combined inventory (grouped by medicine across all Branches)
            var combinedInventory = inventories
                .GroupBy(i => i.MedicineId)
                .Select(g => {
                    var firstItem = g.First();
                    var Branchestocks = g.Select(i => new BranchStockDto
                    {
                        BranchId = i.BranchId,
                        BranchName = i.Branch.Name,
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
                        Branchestocks = Branchestocks
                    };
                })
                .OrderBy(i => i.MedicineName)
                .ToList();

            // Build clinic inventory
            var clinicInventory = Branches.Select(clinic => {
                var Branchestocks = inventories
                    .Where(i => i.BranchId == clinic.Id)
                    .Select(i => new BranchStockDto
                    {
                        BranchId = i.BranchId,
                        BranchName = i.Branch.Name,
                        Quantity = i.CurrentStock,
                        AvailableQuantity = i.CurrentStock,
                        ReservedQuantity = 0, // Not tracked in current model
                        UnitPrice = i.SellingPrice,
                        TotalValue = i.CurrentStock * i.SellingPrice,
                        ReorderLevel = i.MinimumStock,
                        IsLowStock = i.CurrentStock <= i.MinimumStock
                    }).ToList();

                return new BranchInventoryDto
                {
                    BranchId = clinic.Id,
                    BranchName = clinic.Name,
                    MedicineCount = Branchestocks.Count,
                    TotalValue = Branchestocks.Sum(s => s.TotalValue),
                    LowStockCount = Branchestocks.Count(s => s.IsLowStock && s.AvailableQuantity > 0),
                    OutOfStockCount = Branchestocks.Count(s => s.AvailableQuantity <= 0),
                    Stocks = Branchestocks.OrderBy(s => s.IsLowStock ? 0 : 1).ThenBy(s => s.BranchName).ToList()
                };
            }).ToList();

            // Build low stock alerts
            var lowStockAlerts = inventories
                .Where(i => i.CurrentStock <= i.MinimumStock)
                .Select(i => new LowStockAlertDto
                {
                    BranchId = i.BranchId,
                    BranchName = i.Branch.Name,
                    MedicineId = i.MedicineId,
                    MedicineName = i.Medicine?.Name ?? string.Empty,
                    MedicineCode = i.Medicine?.Id.ToString() ?? string.Empty, // Using ID as code since Code field doesn't exist
                    CurrentStock = i.CurrentStock,
                    ReorderLevel = i.MinimumStock,
                    RequiredQuantity = Math.Max(0, i.MinimumStock - i.CurrentStock + (i.MinimumStock * 2)), // Suggest 2x minimum stock
                    Unit = "units" // Unit not stored in ClinicMedicine, using default
                })
                .OrderBy(a => a.CurrentStock)
                .ThenBy(a => a.BranchName)
                .ToList();

            // Build stock movement summary
            var stockMovements = recentTransactions.Select(t => new StockMovementDto
            {
                Date = t.TransactionDate,
                TransactionType = t.TransactionType.ToString(),
                BranchName = t.Branch?.Name ?? string.Empty,
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

