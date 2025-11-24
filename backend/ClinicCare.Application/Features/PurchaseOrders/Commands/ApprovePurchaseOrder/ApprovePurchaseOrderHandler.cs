using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.PurchaseOrders.Queries.GetPurchaseOrder;
using ClinicCare.Application.Features.PurchaseOrders.Queries.GetPurchaseOrders;
using ClinicCare.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.PurchaseOrders.Commands.ApprovePurchaseOrder;

public class ApprovePurchaseOrderHandler : IRequestHandler<ApprovePurchaseOrderCommand, Result<PurchaseOrderDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediator _mediator;

    public ApprovePurchaseOrderHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IMediator mediator)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mediator = mediator;
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

            var getQuery = new GetPurchaseOrderQuery { Id = purchaseOrder.Id };
            var result = await _mediator.Send(getQuery, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            return Result<PurchaseOrderDto>.Failure($"Failed to approve purchase order: {ex.Message}");
        }
    }
}

