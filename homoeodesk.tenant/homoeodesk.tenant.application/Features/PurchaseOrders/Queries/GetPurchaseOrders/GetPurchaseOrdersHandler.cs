using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.PurchaseOrders.Queries.GetPurchaseOrders;

public class GetPurchaseOrdersHandler : IRequestHandler<GetPurchaseOrdersQuery, Result<List<PurchaseOrderDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetPurchaseOrdersHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<PurchaseOrderDto>>> Handle(GetPurchaseOrdersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
            {
                return Result<List<PurchaseOrderDto>>.Failure("User not associated with any organization");
            }

            var query = _context.PurchaseOrders
                .Include(po => po.Branch)
                .Include(po => po.Supplier)
                .Include(po => po.ApprovedByUser)
                .Include(po => po.ReceivedByUser)
                .Include(po => po.Items)
                    .ThenInclude(item => item.Medicine)
                .Where(po => po.OrganizationId == organizationId.Value && po.IsActive);

            if (request.BranchId.HasValue)
            {
                query = query.Where(po => po.BranchId == request.BranchId.Value);
            }

            if (request.SupplierId.HasValue)
            {
                query = query.Where(po => po.SupplierId == request.SupplierId.Value);
            }

            if (request.Status.HasValue)
            {
                query = query.Where(po => (int)po.Status == request.Status.Value);
            }

            if (request.StartDate.HasValue)
            {
                query = query.Where(po => po.OrderDate >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                query = query.Where(po => po.OrderDate <= request.EndDate.Value);
            }

            var purchaseOrders = await query
                .OrderByDescending(po => po.OrderDate)
                .ThenByDescending(po => po.Id)
                .ToListAsync(cancellationToken);

            var dtos = purchaseOrders.Select(po => new PurchaseOrderDto
            {
                Id = po.Id,
                BranchId = po.BranchId,
                BranchName = po.Branch.Name,
                SupplierId = po.SupplierId,
                SupplierName = po.Supplier.Name,
                OrderNumber = po.OrderNumber,
                OrderDate = po.OrderDate,
                ExpectedDeliveryDate = po.ExpectedDeliveryDate,
                Status = (int)po.Status,
                StatusText = po.Status switch
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
                TotalAmount = po.TotalAmount,
                DiscountAmount = po.DiscountAmount,
                TaxAmount = po.TaxAmount,
                GrandTotal = po.GrandTotal,
                Notes = po.Notes,
                ApprovedDate = po.ApprovedDate,
                ApprovedByUserId = po.ApprovedByUserId,
                ApprovedByUserName = po.ApprovedByUser != null 
                    ? $"{po.ApprovedByUser.FirstName} {po.ApprovedByUser.LastName}" 
                    : null,
                OrderedDate = po.OrderedDate,
                ReceivedDate = po.ReceivedDate,
                ReceivedByUserId = po.ReceivedByUserId,
                ReceivedByUserName = po.ReceivedByUser != null 
                    ? $"{po.ReceivedByUser.FirstName} {po.ReceivedByUser.LastName}" 
                    : null,
                Items = po.Items.Select(item => new PurchaseOrderItemDto
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
            }).ToList();

            return Result<List<PurchaseOrderDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<PurchaseOrderDto>>.Failure($"Failed to retrieve purchase orders: {ex.Message}");
        }
    }
}

