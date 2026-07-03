using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.PurchaseOrders.Queries.GetPurchaseOrder;
using HomoeoDesk.Tenant.Application.Features.PurchaseOrders.Queries.GetPurchaseOrders;
using HomoeoDesk.Tenant.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.PurchaseOrders.Commands.CancelPurchaseOrder;

public class CancelPurchaseOrderHandler : IRequestHandler<CancelPurchaseOrderCommand, Result<PurchaseOrderDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediator _mediator;

    public CancelPurchaseOrderHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IMediator mediator)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mediator = mediator;
    }

    public async Task<Result<PurchaseOrderDto>> Handle(CancelPurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
            {
                return Result<PurchaseOrderDto>.Failure("User not associated with any organization");
            }

            var purchaseOrder = await _context.PurchaseOrders
                .FirstOrDefaultAsync(po => po.Id == request.Id 
                    && po.OrganizationId == organizationId.Value 
                    && po.IsActive, cancellationToken);

            if (purchaseOrder == null)
            {
                return Result<PurchaseOrderDto>.Failure("Purchase order not found");
            }

            if (purchaseOrder.Status == PurchaseOrderStatus.Received)
            {
                return Result<PurchaseOrderDto>.Failure("Cannot cancel a fully received purchase order");
            }

            if (purchaseOrder.Status == PurchaseOrderStatus.Cancelled)
            {
                return Result<PurchaseOrderDto>.Failure("Purchase order is already cancelled");
            }

            purchaseOrder.Status = PurchaseOrderStatus.Cancelled;
            if (!string.IsNullOrEmpty(request.Reason))
            {
                purchaseOrder.Notes = string.IsNullOrEmpty(purchaseOrder.Notes) 
                    ? $"Cancelled: {request.Reason}" 
                    : $"{purchaseOrder.Notes}\nCancelled: {request.Reason}";
            }
            purchaseOrder.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            var getQuery = new GetPurchaseOrderQuery { Id = purchaseOrder.Id };
            var result = await _mediator.Send(getQuery, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            return Result<PurchaseOrderDto>.Failure($"Failed to cancel purchase order: {ex.Message}");
        }
    }
}

