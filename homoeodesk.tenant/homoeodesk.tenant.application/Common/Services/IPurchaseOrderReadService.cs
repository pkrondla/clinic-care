using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.PurchaseOrders.Queries.GetPurchaseOrders;

namespace HomoeoDesk.Tenant.Application.Common.Services;

/// <summary>
/// Builds the full PurchaseOrderDto for a given purchase order. Shared by GetPurchaseOrderHandler
/// and the Create/Approve/Receive/Cancel handlers so they don't round-trip through MediatR to
/// fetch their own response DTO.
/// </summary>
public interface IPurchaseOrderReadService
{
    Task<Result<PurchaseOrderDto>> GetPurchaseOrderDtoAsync(int purchaseOrderId, int organizationId, CancellationToken cancellationToken);
}
