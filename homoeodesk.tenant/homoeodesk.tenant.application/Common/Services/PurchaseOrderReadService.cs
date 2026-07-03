using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.PurchaseOrders.Queries.GetPurchaseOrders;
using HomoeoDesk.Tenant.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Common.Services;

public class PurchaseOrderReadService : IPurchaseOrderReadService
{
    private readonly IApplicationDbContext _context;

    public PurchaseOrderReadService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PurchaseOrderDto>> GetPurchaseOrderDtoAsync(int purchaseOrderId, int organizationId, CancellationToken cancellationToken)
    {
        var purchaseOrder = await _context.PurchaseOrders
            .Include(po => po.Branch)
            .Include(po => po.Supplier)
            .Include(po => po.ApprovedByUser)
            .Include(po => po.ReceivedByUser)
            .Include(po => po.Items)
                .ThenInclude(item => item.Medicine)
            .FirstOrDefaultAsync(po => po.Id == purchaseOrderId
                && po.OrganizationId == organizationId
                && po.IsActive, cancellationToken);

        if (purchaseOrder == null)
        {
            return Result<PurchaseOrderDto>.Failure("Purchase order not found");
        }

        var dto = new PurchaseOrderDto
        {
            Id = purchaseOrder.Id,
            BranchId = purchaseOrder.BranchId,
            BranchName = purchaseOrder.Branch.Name,
            SupplierId = purchaseOrder.SupplierId,
            SupplierName = purchaseOrder.Supplier.Name,
            OrderNumber = purchaseOrder.OrderNumber,
            OrderDate = purchaseOrder.OrderDate,
            ExpectedDeliveryDate = purchaseOrder.ExpectedDeliveryDate,
            Status = (int)purchaseOrder.Status,
            StatusText = purchaseOrder.Status switch
            {
                PurchaseOrderStatus.Draft => "Draft",
                PurchaseOrderStatus.Pending => "Pending",
                PurchaseOrderStatus.Approved => "Approved",
                PurchaseOrderStatus.Ordered => "Ordered",
                PurchaseOrderStatus.PartiallyReceived => "Partially Received",
                PurchaseOrderStatus.Received => "Received",
                PurchaseOrderStatus.Cancelled => "Cancelled",
                _ => "Unknown"
            },
            TotalAmount = purchaseOrder.TotalAmount,
            DiscountAmount = purchaseOrder.DiscountAmount,
            TaxAmount = purchaseOrder.TaxAmount,
            GrandTotal = purchaseOrder.GrandTotal,
            Notes = purchaseOrder.Notes,
            ApprovedDate = purchaseOrder.ApprovedDate,
            ApprovedByUserId = purchaseOrder.ApprovedByUserId,
            ApprovedByUserName = purchaseOrder.ApprovedByUser != null
                ? $"{purchaseOrder.ApprovedByUser.FirstName} {purchaseOrder.ApprovedByUser.LastName}"
                : null,
            OrderedDate = purchaseOrder.OrderedDate,
            ReceivedDate = purchaseOrder.ReceivedDate,
            ReceivedByUserId = purchaseOrder.ReceivedByUserId,
            ReceivedByUserName = purchaseOrder.ReceivedByUser != null
                ? $"{purchaseOrder.ReceivedByUser.FirstName} {purchaseOrder.ReceivedByUser.LastName}"
                : null,
            Items = purchaseOrder.Items.Select(item => new PurchaseOrderItemDto
            {
                Id = item.Id,
                MedicineId = item.MedicineId,
                MedicineName = item.Medicine.Name,
                Quantity = item.Quantity,
                ReceivedQuantity = item.ReceivedQuantity,
                UnitPrice = item.UnitPrice,
                DiscountAmount = item.DiscountAmount,
                TotalPrice = item.TotalPrice,
                BatchNumber = item.BatchNumber,
                ExpiryDate = item.ExpiryDate,
                Notes = item.Notes
            }).ToList()
        };

        return Result<PurchaseOrderDto>.Success(dto);
    }
}
