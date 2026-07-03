using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Common.Services;
using HomoeoDesk.Tenant.Application.Features.PurchaseOrders.Queries.GetPurchaseOrders;
using HomoeoDesk.Tenant.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.PurchaseOrders.Commands.ApprovePurchaseOrder;

public class ApprovePurchaseOrderHandler : IRequestHandler<ApprovePurchaseOrderCommand, Result<PurchaseOrderDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPurchaseOrderReadService _purchaseOrderReadService;

    public ApprovePurchaseOrderHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IPurchaseOrderReadService purchaseOrderReadService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _purchaseOrderReadService = purchaseOrderReadService;
    }

    public async Task<Result<PurchaseOrderDto>> Handle(ApprovePurchaseOrderCommand request, CancellationToken cancellationToken)
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
                .FirstOrDefaultAsync(po => po.Id == request.Id 
                    && po.OrganizationId == organizationId.Value 
                    && po.IsActive, cancellationToken);

            if (purchaseOrder == null)
            {
                return Result<PurchaseOrderDto>.Failure("Purchase order not found");
            }

            if (purchaseOrder.Status != PurchaseOrderStatus.Draft && purchaseOrder.Status != PurchaseOrderStatus.Pending)
            {
                return Result<PurchaseOrderDto>.Failure("Only draft or pending purchase orders can be approved");
            }

            purchaseOrder.Status = PurchaseOrderStatus.Approved;
            purchaseOrder.ApprovedDate = DateTime.UtcNow;
            purchaseOrder.ApprovedByUserId = userId.Value;
            purchaseOrder.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return await _purchaseOrderReadService.GetPurchaseOrderDtoAsync(purchaseOrder.Id, organizationId.Value, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<PurchaseOrderDto>.Failure($"Failed to approve purchase order: {ex.Message}");
        }
    }
}

