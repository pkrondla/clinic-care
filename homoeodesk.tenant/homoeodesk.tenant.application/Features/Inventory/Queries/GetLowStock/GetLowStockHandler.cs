using AutoMapper;
using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Inventory.Commands.AdjustStock;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Inventory.Queries.GetLowStock;

public class GetLowStockHandler : IRequestHandler<GetLowStockQuery, Result<List<InventoryDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetLowStockHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<List<InventoryDto>>> Handle(GetLowStockQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var BranchId = request.BranchId ?? 0;
            if (BranchId == 0)
            {
                return Result<List<InventoryDto>>.Failure(new[] { "Clinic ID is required." });
            }

            var lowStock = await _context.Inventories
                .Include(i => i.Medicine)
                .Where(i => i.BranchId == BranchId && i.CurrentStock <= i.MinimumStock)
                .OrderBy(i => i.CurrentStock)
                .ToListAsync(cancellationToken);

            var dtos = _mapper.Map<List<InventoryDto>>(lowStock);

            return Result<List<InventoryDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<InventoryDto>>.Failure(new[] { $"Failed to retrieve low stock items: {ex.Message}" });
        }
    }
}
