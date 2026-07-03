using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Common.Services;
using HomoeoDesk.Tenant.Application.Features.PurchaseOrders.Queries.GetPurchaseOrders;
using HomoeoDesk.Tenant.Domain.Entities;
using HomoeoDesk.Tenant.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.PurchaseOrders.Commands.CreatePurchaseOrder;

public class CreatePurchaseOrderHandler : IRequestHandler<CreatePurchaseOrderCommand, Result<PurchaseOrderDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPurchaseOrderReadService _purchaseOrderReadService;

    public CreatePurchaseOrderHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IPurchaseOrderReadService purchaseOrderReadService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _purchaseOrderReadService = purchaseOrderReadService;
    }

    public async Task<Result<PurchaseOrderDto>> Handle(CreatePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
            {
                return Result<PurchaseOrderDto>.Failure("User not associated with any organization");
            }

            // Validate clinic
            var clinic = await _context.Branches
                .FirstOrDefaultAsync(c => c.Id == request.BranchId 
                    && c.OrganizationId == organizationId.Value 
                    && c.IsActive, cancellationToken);

            if (clinic == null)
            {
                return Result<PurchaseOrderDto>.Failure("Branch not found");
            }

            // Validate supplier
            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.Id == request.SupplierId 
                    && s.OrganizationId == organizationId.Value 
                    && s.IsActive, cancellationToken);

            if (supplier == null)
            {
                return Result<PurchaseOrderDto>.Failure("Supplier not found");
            }

            // Validate medicines
            var medicineIds = request.Items.Select(i => i.MedicineId).ToList();
            var medicines = await _context.ClinicMedicines
                .Where(m => medicineIds.Contains(m.Id) 
                    && m.OrganizationId == organizationId.Value 
                    && m.IsActive)
                .ToListAsync(cancellationToken);

            if (medicines.Count != medicineIds.Count)
            {
                return Result<PurchaseOrderDto>.Failure("One or more medicines not found");
            }

            // Generate order number
            var orderNumber = await GenerateOrderNumberAsync(organizationId.Value, cancellationToken);

            // Calculate totals
            decimal totalAmount = 0;
            decimal totalDiscount = request.DiscountAmount ?? 0;
            decimal totalTax = request.TaxAmount ?? 0;

            foreach (var item in request.Items)
            {
                var itemTotal = item.Quantity * item.UnitPrice;
                var itemDiscount = item.DiscountAmount ?? 0;
                totalAmount += itemTotal - itemDiscount;
            }

            var grandTotal = totalAmount - totalDiscount + totalTax;

            // Create purchase order
            var purchaseOrder = new PurchaseOrder
            {
                OrganizationId = organizationId.Value,
                BranchId = request.BranchId,
                SupplierId = request.SupplierId,
                OrderNumber = orderNumber,
                OrderDate = request.OrderDate ?? DateTime.UtcNow,
                ExpectedDeliveryDate = request.ExpectedDeliveryDate,
                Status = PurchaseOrderStatus.Draft,
                TotalAmount = totalAmount,
                DiscountAmount = totalDiscount,
                TaxAmount = totalTax,
                GrandTotal = grandTotal,
                Notes = request.Notes,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.PurchaseOrders.Add(purchaseOrder);
            await _context.SaveChangesAsync(cancellationToken);

            // Create purchase order items
            foreach (var itemCommand in request.Items)
            {
                var itemTotal = itemCommand.Quantity * itemCommand.UnitPrice;
                var itemDiscount = itemCommand.DiscountAmount ?? 0;

                var item = new PurchaseOrderItem
                {
                    OrganizationId = organizationId.Value,
                    PurchaseOrderId = purchaseOrder.Id,
                    MedicineId = itemCommand.MedicineId,
                    Quantity = itemCommand.Quantity,
                    UnitPrice = itemCommand.UnitPrice,
                    DiscountAmount = itemDiscount,
                    TotalPrice = itemTotal - itemDiscount,
                    BatchNumber = itemCommand.BatchNumber,
                    ExpiryDate = itemCommand.ExpiryDate,
                    Notes = itemCommand.Notes,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.PurchaseOrderItems.Add(item);
            }

            await _context.SaveChangesAsync(cancellationToken);

            // Return the created purchase order
            return await _purchaseOrderReadService.GetPurchaseOrderDtoAsync(purchaseOrder.Id, organizationId.Value, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<PurchaseOrderDto>.Failure($"Failed to create purchase order: {ex.Message}");
        }
    }

    private async Task<string> GenerateOrderNumberAsync(int organizationId, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow;
        var prefix = $"PO{today:yyyyMMdd}";

        var lastOrder = await _context.PurchaseOrders
            .Where(po => po.OrderNumber.StartsWith(prefix) && po.OrganizationId == organizationId)
            .OrderByDescending(po => po.OrderNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastOrder == null)
        {
            return $"{prefix}0001";
        }

        var lastNumber = lastOrder.OrderNumber.Substring(prefix.Length);
        if (int.TryParse(lastNumber, out var sequence))
        {
            return $"{prefix}{(sequence + 1):D4}";
        }

        return $"{prefix}0001";
    }
}

