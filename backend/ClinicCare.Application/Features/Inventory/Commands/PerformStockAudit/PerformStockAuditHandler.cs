using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Models;
using ClinicCare.Domain.Entities;
using ClinicCare.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.Inventory.Commands.PerformStockAudit;

public class PerformStockAuditHandler : IRequestHandler<PerformStockAuditCommand, Result<StockAuditResultDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediator _mediator;

    public PerformStockAuditHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IMediator mediator)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mediator = mediator;
    }

    public async Task<Result<StockAuditResultDto>> Handle(PerformStockAuditCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            var userId = _currentUserService.UserId;

            if (!organizationId.HasValue || !userId.HasValue)
            {
                return Result<StockAuditResultDto>.Failure("User not authenticated");
            }

            // Validate clinic
            var clinic = await _context.Clinics
                .FirstOrDefaultAsync(c => c.Id == request.ClinicId 
                    && c.OrganizationId == organizationId.Value 
                    && c.IsActive, cancellationToken);

            if (clinic == null)
            {
                return Result<StockAuditResultDto>.Failure("Clinic not found");
            }

            var auditDate = DateTime.UtcNow;
            var auditItems = new List<StockAuditItemResultDto>();
            int itemsWithVariance = 0;

            // Process each audit item
            foreach (var auditItem in request.AuditItems)
            {
                var inventory = await _context.Inventories
                    .Include(i => i.Medicine)
                    .FirstOrDefaultAsync(i => i.Id == auditItem.InventoryId 
                        && i.ClinicId == request.ClinicId 
                        && i.OrganizationId == organizationId.Value 
                        && i.IsActive, cancellationToken);

                if (inventory == null)
                {
                    continue; // Skip invalid items
                }

                var originalSystemStock = inventory.CurrentStock;
                var variance = auditItem.PhysicalStock - originalSystemStock;
                var hasVariance = variance != 0;

                if (hasVariance)
                {
                    itemsWithVariance++;

                    // Adjust stock to match physical count
                    var adjustmentQuantity = variance;

                    // Update inventory
                    inventory.CurrentStock = auditItem.PhysicalStock;
                    inventory.LastUpdated = auditDate;
                    inventory.UpdatedAt = auditDate;

                    // Create stock transaction for audit adjustment
                    var stockTransaction = new StockTransaction
                    {
                        OrganizationId = organizationId.Value,
                        ClinicId = request.ClinicId,
                        MedicineId = inventory.MedicineId,
                        TransactionType = TransactionType.Adjustment,
                        Quantity = Math.Abs(adjustmentQuantity),
                        UnitPrice = inventory.PurchasePrice,
                        Reference = $"STOCK_AUDIT_{auditDate:yyyyMMdd}_{Guid.NewGuid():N}",
                        Notes = $"Stock audit adjustment. Physical: {auditItem.PhysicalStock}, System: {originalSystemStock}. {auditItem.Notes ?? ""}",
                        TransactionDate = auditDate,
                        IsActive = true,
                        CreatedAt = auditDate,
                        UpdatedAt = auditDate
                    };

                    _context.StockTransactions.Add(stockTransaction);
                }

                auditItems.Add(new StockAuditItemResultDto
                {
                    InventoryId = inventory.Id,
                    MedicineId = inventory.MedicineId,
                    MedicineName = inventory.Medicine.Name,
                    SystemStock = originalSystemStock,
                    PhysicalStock = auditItem.PhysicalStock,
                    Variance = variance,
                    StockAdjusted = hasVariance,
                    Notes = auditItem.Notes
                });
            }

            // Save all changes
            await _context.SaveChangesAsync(cancellationToken);

            var result = new StockAuditResultDto
            {
                ClinicId = request.ClinicId,
                ClinicName = clinic.Name,
                AuditDate = auditDate,
                TotalItemsAudited = auditItems.Count,
                ItemsWithVariance = itemsWithVariance,
                Items = auditItems,
                Notes = request.Notes
            };

            return Result<StockAuditResultDto>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<StockAuditResultDto>.Failure($"Failed to perform stock audit: {ex.Message}");
        }
    }
}

