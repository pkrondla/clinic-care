using AutoMapper;
using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Inventory.Commands.AdjustStock;

public class AdjustStockHandler : IRequestHandler<AdjustStockCommand, Result<InventoryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public AdjustStockHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<InventoryDto>> Handle(AdjustStockCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var inventory = await _context.Inventories
                .Include(i => i.Medicine)
                .Include(i => i.Branch)
                .FirstOrDefaultAsync(i => i.Id == request.InventoryId, cancellationToken);

            if (inventory == null)
            {
                return Result<InventoryDto>.Failure(new[] { $"Inventory item with ID {request.InventoryId} not found." });
            }

            switch (request.TransactionType.ToLower())
            {
                case "purchase":
                case "restock":
                case "return":
                    inventory.CurrentStock += request.Quantity;
                    break;

                case "sale":
                case "dispensing":
                case "wastage":
                    inventory.CurrentStock -= request.Quantity;
                    break;

                case "adjustment":
                    inventory.CurrentStock = request.Quantity;
                    break;

                default:
                    return Result<InventoryDto>.Failure(new[] { $"Unknown transaction type: {request.TransactionType}" });
            }

            if (inventory.CurrentStock < 0)
            {
                return Result<InventoryDto>.Failure(new[] { "Insufficient stock. Cannot reduce stock below zero." });
            }

            inventory.LastUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            var dto = _mapper.Map<InventoryDto>(inventory);
            return Result<InventoryDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<InventoryDto>.Failure(new[] { $"Failed to adjust stock: {ex.Message}" });
        }
    }
}
