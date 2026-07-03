using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Common.Services;
using HomoeoDesk.Tenant.Application.Features.PurchaseOrders.Queries.GetPurchaseOrders;
using MediatR;

namespace HomoeoDesk.Tenant.Application.Features.PurchaseOrders.Queries.GetPurchaseOrder;

public class GetPurchaseOrderHandler : IRequestHandler<GetPurchaseOrderQuery, Result<PurchaseOrderDto>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IPurchaseOrderReadService _purchaseOrderReadService;

    public GetPurchaseOrderHandler(
        ICurrentUserService currentUserService,
        IPurchaseOrderReadService purchaseOrderReadService)
    {
        _currentUserService = currentUserService;
        _purchaseOrderReadService = purchaseOrderReadService;
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

            return await _purchaseOrderReadService.GetPurchaseOrderDtoAsync(request.Id, organizationId.Value, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<PurchaseOrderDto>.Failure($"Failed to retrieve purchase order: {ex.Message}");
        }
    }
}

