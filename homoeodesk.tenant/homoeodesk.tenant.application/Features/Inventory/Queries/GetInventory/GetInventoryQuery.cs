using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Inventory.Commands.AdjustStock;
using MediatR;

namespace HomoeoDesk.Tenant.Application.Features.Inventory.Queries.GetInventory;

public class GetInventoryQuery : IRequest<Result<List<InventoryDto>>>
{
    public int? BranchId { get; set; }
}

