using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.PurchaseOrders.Queries.GetPurchaseOrders;
using ClinicCare.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.PurchaseOrders.Queries.GetPurchaseOrder;

public class GetPurchaseOrderHandler : IRequestHandler<GetPurchaseOrderQuery, Result<PurchaseOrderDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetPurchaseOrderHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PurchaseOrderDto>> Handle(GetPurchaseOrderQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
            {
                return Result<PurchaseOrderDto>.Failure("User not associated with any organization");
            }

            var purchaseOrder = await _context.PurchaseOrders
                .Include(po => po.Clinic)
                .Include(po => po.Supplier)
                .Include(po => po.ApprovedByUser)
                .Include(po => po.ReceivedByUser)
                .Include(po => po.Items)
                    .ThenInclude(item => item.Medicine)
                .FirstOrDefaultAsync(po => po.Id == request.Id 
                    && po.OrganizationId == organizationId.Value 
                    && po.IsActive, cancellationToken);

            if (purchaseOrder == null)
            {
                return Result<PurchaseOrderDto>.Failure("Purchase order not found");
            }

            var dto = new PurchaseOrderDto
            {
                Id = purchaseOrder.Id,
                ClinicId = purchaseOrder.ClinicId,
                ClinicName = purchaseOrder.Clinic.Name,
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
        catch (Exception ex)
        {
            return Result<PurchaseOrderDto>.Failure($"Failed to retrieve purchase order: {ex.Message}");
        }
    }
}

