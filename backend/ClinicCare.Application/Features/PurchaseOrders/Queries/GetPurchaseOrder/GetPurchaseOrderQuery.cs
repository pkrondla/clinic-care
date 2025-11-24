using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.PurchaseOrders.Queries.GetPurchaseOrders;
using MediatR;

namespace ClinicCare.Application.Features.PurchaseOrders.Queries.GetPurchaseOrder;

public class GetPurchaseOrderQuery : IRequest<Result<PurchaseOrderDto>>
{
    public int Id { get; set; }
}

