using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.PurchaseOrders.Queries.GetPurchaseOrders;
using MediatR;

namespace HomoeoDesk.Tenant.Application.Features.PurchaseOrders.Queries.GetPurchaseOrder;

public class GetPurchaseOrderQuery : IRequest<Result<PurchaseOrderDto>>
{
    public int Id { get; set; }
}

