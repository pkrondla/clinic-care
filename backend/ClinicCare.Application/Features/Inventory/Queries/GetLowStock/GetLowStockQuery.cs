using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Inventory.Commands.AdjustStock;
using MediatR;

namespace ClinicCare.Application.Features.Inventory.Queries.GetLowStock;

public class GetLowStockQuery : IRequest<Result<List<InventoryDto>>>
{
    public int? ClinicId { get; set; }
}

