using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.PurchaseOrders.Queries.GetPurchaseOrder;
using HomoeoDesk.Tenant.Application.Features.PurchaseOrders.Queries.GetPurchaseOrders;
using HomoeoDesk.Tenant.Domain.Entities;
using HomoeoDesk.Tenant.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.PurchaseOrders.Commands.ReceivePurchaseOrder;

public class ReceivePurchaseOrderHandler : IRequestHandler<ReceivePurchaseOrderCommand, Result<PurchaseOrderDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediator _mediator;

    public ReceivePurchaseOrderHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IMediator mediator)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mediator = mediator;
    }

    public async Task<Result<PurchaseOrderDto>> Handle(ReceivePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            var userId = _currentUserService.UserId;

            if (!organizationId.HasValue || !userId.HasValue)
            {
                return Result<PurchaseOrderDto>.Failure("User not authenticated");
            }

            var purchaseOrder = await _context.PurchaseOrders
                .Include(po => po.Items)
                .FirstOrDefaultAsync(po => po.Id == request.Id 
                    && po.OrganizationId == organizationId.Value 
                    && po.IsActive, cancellationToken);

            if (purchaseOrder == null)
            {
                return Result<PurchaseOrderDto>.Failure("Purchase order not found");
            }

            if (purchaseOrder.Status != PurchaseOrderStatus.Approved && purchaseOrder.Status != PurchaseOrderStatus.Ordered && purchaseOrder.Status != PurchaseOrderStatus.PartiallyReceived)
            {
                return Result<PurchaseOrderDto>.Failure("Only approved, ordered, or partially received purchase orders can be received");
            }

            // Update received quantities and update inventory
            bool allReceived = true;
            bool someReceived = false;

            foreach (var receivedItem in request.ReceivedItems)
            {
                var orderItem = purchaseOrder.Items.FirstOrDefault(i => i.Id == receivedItem.PurchaseOrderItemId);
                if (orderItem == null)
                {
                    return Result<PurchaseOrderDto>.Failure($"Purchase order item {receivedItem.PurchaseOrderItemId} not found");
                }

                if (receivedItem.ReceivedQuantity > orderItem.Quantity - (orderItem.ReceivedQuantity ?? 0))
                {
                    return Result<PurchaseOrderDto>.Failure($"Received quantity cannot exceed ordered quantity for item {orderItem.MedicineId}");
                }

                orderItem.ReceivedQuantity = (orderItem.ReceivedQuantity ?? 0) + receivedItem.ReceivedQuantity;
                if (!string.IsNullOrEmpty(receivedItem.BatchNumber))
                {
                    orderItem.BatchNumber = receivedItem.BatchNumber;
                }
                if (receivedItem.ExpiryDate.HasValue)
                {
                    orderItem.ExpiryDate = receivedItem.ExpiryDate;
                }
                orderItem.UpdatedAt = DateTime.UtcNow;

                // Update inventory - find by medicine and clinic
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.BranchId == purchaseOrder.BranchId 
                        && i.MedicineId == orderItem.MedicineId 
                        && i.OrganizationId == organizationId.Value 
                        && i.IsActive, cancellationToken);

                if (inventory != null)
                {
                    inventory.CurrentStock += receivedItem.ReceivedQuantity;
                    inventory.PurchasePrice = orderItem.UnitPrice;
                    if (receivedItem.ExpiryDate.HasValue)
                    {
                        inventory.ExpiryDate = receivedItem.ExpiryDate.Value;
                    }
                    if (!string.IsNullOrEmpty(receivedItem.BatchNumber))
                    {
                        inventory.BatchNumber = receivedItem.BatchNumber;
                    }
                    inventory.LastUpdated = DateTime.UtcNow;
                    inventory.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // Create new inventory item
                    var medicine = await _context.ClinicMedicines
                        .FirstOrDefaultAsync(m => m.Id == orderItem.MedicineId 
                            && m.OrganizationId == organizationId.Value 
                            && m.IsActive, cancellationToken);

                    if (medicine == null)
                    {
                        return Result<PurchaseOrderDto>.Failure($"Medicine {orderItem.MedicineId} not found");
                    }

                    var newInventory = new Domain.Entities.Inventory
                    {
                        OrganizationId = organizationId.Value,
                        BranchId = purchaseOrder.BranchId,
                        MedicineId = orderItem.MedicineId,
                        CurrentStock = receivedItem.ReceivedQuantity,
                        MinimumStock = 0,
                        MaximumStock = 0,
                        PurchasePrice = orderItem.UnitPrice,
                        SellingPrice = medicine.SellingPrice,
                        ExpiryDate = receivedItem.ExpiryDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
                        BatchNumber = receivedItem.BatchNumber ?? string.Empty,
                        LastUpdated = DateTime.UtcNow,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Inventories.Add(newInventory);
                }

                // Create stock transaction
                var stockTransaction = new StockTransaction
                {
                    OrganizationId = organizationId.Value,
                    BranchId = purchaseOrder.BranchId,
                    MedicineId = orderItem.MedicineId,
                    TransactionType = TransactionType.Purchase,
                    Quantity = receivedItem.ReceivedQuantity,
                    UnitPrice = orderItem.UnitPrice,
                    Reference = purchaseOrder.OrderNumber,
                    Notes = $"Received from Purchase Order {purchaseOrder.OrderNumber}",
                    TransactionDate = DateTime.UtcNow,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.StockTransactions.Add(stockTransaction);

                // Check if all items are fully received
                if (orderItem.ReceivedQuantity < orderItem.Quantity)
                {
                    allReceived = false;
                }
                if (orderItem.ReceivedQuantity > 0)
                {
                    someReceived = true;
                }
            }

            // Update purchase order status
            if (allReceived)
            {
                purchaseOrder.Status = PurchaseOrderStatus.Received;
                purchaseOrder.ReceivedDate = DateTime.UtcNow;
            }
            else if (someReceived)
            {
                purchaseOrder.Status = PurchaseOrderStatus.PartiallyReceived;
            }

            purchaseOrder.ReceivedByUserId = userId.Value;
            purchaseOrder.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            var getQuery = new GetPurchaseOrderQuery { Id = purchaseOrder.Id };
            var result = await _mediator.Send(getQuery, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            return Result<PurchaseOrderDto>.Failure($"Failed to receive purchase order: {ex.Message}");
        }
    }
}

