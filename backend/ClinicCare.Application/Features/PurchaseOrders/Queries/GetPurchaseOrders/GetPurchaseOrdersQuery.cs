using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.PurchaseOrders.Queries.GetPurchaseOrders;
using MediatR;

namespace ClinicCare.Application.Features.PurchaseOrders.Queries.GetPurchaseOrders;

public class GetPurchaseOrdersQuery : IRequest<Result<List<PurchaseOrderDto>>>
{
    public int? ClinicId { get; set; }
    public int? SupplierId { get; set; }
    public int? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

