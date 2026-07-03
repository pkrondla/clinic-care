using AutoMapper;
using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Inventory.Commands.AdjustStock;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Inventory.Commands.CreateInventoryItem;

public class CreateInventoryItemHandler : IRequestHandler<CreateInventoryItemCommand, Result<InventoryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateInventoryItemHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<InventoryDto>> Handle(CreateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var BranchId = request.BranchId;
            if (BranchId == 0)
            {
                return Result<InventoryDto>.Failure(new[] { "Clinic ID is required." });
            }

            var existing = await _context.Inventories
                .Include(i => i.Medicine)
                .FirstOrDefaultAsync(i => i.BranchId == BranchId && i.MedicineId == request.MedicineId, cancellationToken);

            if (existing != null)
            {
                return Result<InventoryDto>.Failure(new[] { "Inventory item for this medicine already exists in this branch." });
            }

            var inventory = new Domain.Entities.Inventory
            {
                BranchId = BranchId,
                MedicineId = request.MedicineId,
                CurrentStock = request.InitialStock,
                MinimumStock = request.MinimumStock,
                MaximumStock = request.MaximumStock,
                PurchasePrice = request.PurchasePrice,
                SellingPrice = request.SellingPrice,
                ExpiryDate = request.ExpiryDate,
                BatchNumber = request.BatchNumber ?? string.Empty,
                LastUpdated = DateTime.UtcNow
            };

            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync(cancellationToken);

            var dto = _mapper.Map<InventoryDto>(inventory);

            return Result<InventoryDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<InventoryDto>.Failure(new[] { $"Failed to create inventory item: {ex.Message}" });
        }
    }
}
