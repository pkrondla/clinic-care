using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Inventory.Queries.GetStockAuditHistory;

public class GetStockAuditHistoryHandler : IRequestHandler<GetStockAuditHistoryQuery, Result<List<StockAuditHistoryDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetStockAuditHistoryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<StockAuditHistoryDto>>> Handle(GetStockAuditHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
            {
                return Result<List<StockAuditHistoryDto>>.Failure("User not associated with any organization");
            }

            // Get stock audit transactions (adjustments with audit reference)
            var query = _context.StockTransactions
                .Include(t => t.Branch)
                .Include(t => t.Medicine)
                .Where(t => t.OrganizationId == organizationId.Value 
                    && t.TransactionType == TransactionType.Adjustment
                    && t.Reference.StartsWith("STOCK_AUDIT_")
                    && t.IsActive);

            if (request.BranchId.HasValue)
            {
                query = query.Where(t => t.BranchId == request.BranchId.Value);
            }

            if (request.StartDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate <= request.EndDate.Value);
            }

            var transactions = await query
                .OrderByDescending(t => t.TransactionDate)
                .ThenByDescending(t => t.Id)
                .ToListAsync(cancellationToken);

            // Group by audit reference to reconstruct audit sessions
            var auditGroups = transactions
                .GroupBy(t => t.Reference)
                .ToList();

            var auditHistory = new List<StockAuditHistoryDto>();

            foreach (var group in auditGroups)
            {
                var firstTransaction = group.First();
                var auditDate = firstTransaction.TransactionDate;

                // For each transaction in the audit, create a history entry
                foreach (var transaction in group)
                {
                    // Extract system stock from notes (if available) or calculate from current stock
                    var systemStock = 0;
                    var physicalStock = transaction.Quantity;
                    var variance = 0;

                    // Try to parse notes to extract stock information
                    if (!string.IsNullOrEmpty(transaction.Notes))
                    {
                        // Notes format: "Stock audit adjustment. Physical: X, System: Y."
                        var notesParts = transaction.Notes.Split('.');
                        foreach (var part in notesParts)
                        {
                            if (part.Contains("Physical:"))
                            {
                                var physicalMatch = System.Text.RegularExpressions.Regex.Match(part, @"Physical:\s*(\d+)");
                                if (physicalMatch.Success)
                                {
                                    int.TryParse(physicalMatch.Groups[1].Value, out physicalStock);
                                }
                            }
                            if (part.Contains("System:"))
                            {
                                var systemMatch = System.Text.RegularExpressions.Regex.Match(part, @"System:\s*(\d+)");
                                if (systemMatch.Success)
                                {
                                    int.TryParse(systemMatch.Groups[1].Value, out systemStock);
                                }
                            }
                        }
                    }

                    // If we couldn't parse from notes, estimate from transaction
                    if (systemStock == 0)
                    {
                        // Get current inventory to estimate
                        var inventory = await _context.Inventories
                            .FirstOrDefaultAsync(i => i.MedicineId == transaction.MedicineId 
                                && i.BranchId == transaction.BranchId 
                                && i.OrganizationId == organizationId.Value 
                                && i.IsActive, cancellationToken);

                        if (inventory != null)
                        {
                            // Estimate: current stock + adjustment = previous stock
                            systemStock = inventory.CurrentStock - transaction.Quantity;
                            physicalStock = inventory.CurrentStock;
                        }
                    }

                    variance = physicalStock - systemStock;

                    auditHistory.Add(new StockAuditHistoryDto
                    {
                        Id = transaction.Id,
                        BranchId = transaction.BranchId,
                        BranchName = transaction.Branch.Name,
                        MedicineId = transaction.MedicineId,
                        MedicineName = transaction.Medicine.Name,
                        SystemStock = systemStock,
                        PhysicalStock = physicalStock,
                        Variance = variance,
                        AuditDate = auditDate,
                        AuditedByUserId = null, // Not tracked in current implementation
                        AuditedByUserName = null,
                        Notes = transaction.Notes,
                        Reference = transaction.Reference
                    });
                }
            }

            return Result<List<StockAuditHistoryDto>>.Success(auditHistory);
        }
        catch (Exception ex)
        {
            return Result<List<StockAuditHistoryDto>>.Failure($"Failed to retrieve stock audit history: {ex.Message}");
        }
    }
}

