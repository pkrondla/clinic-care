using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Inventory.Commands.AdjustStock;
using MediatR;

namespace HomoeoDesk.Tenant.Application.Features.Inventory.Queries.GetLowStock;

public class GetLowStockQuery : IRequest<Result<List<InventoryDto>>>
{
    public int? BranchId { get; set; }
}

