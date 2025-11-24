using AutoMapper;
using ClinicCare.Application.Common.Interfaces.Tenant;
using ClinicCare.Application.Common.Models;
using MediatR;

namespace ClinicCare.Application.Features.Inventory.Commands.AdjustStock;

public class AdjustStockHandler : IRequestHandler<AdjustStockCommand, Result<InventoryDto>>
{
    private readonly IInventoryRepository _repository;
    private readonly IMapper _mapper;

    public AdjustStockHandler(IInventoryRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<InventoryDto>> Handle(AdjustStockCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get existing inventory item
            var inventory = await _repository.GetByIdAsync(request.InventoryId, cancellationToken);
            if (inventory == null)
            {
                return Result<InventoryDto>.Failure(new[] { $"Inventory item with ID {request.InventoryId} not found." });
            }

            // Validate stock reduction doesn't go negative
            var newQuantity = inventory.CurrentStock + request.Quantity;
            if (newQuantity < 0)
            {
                return Result<InventoryDto>.Failure(new[] { "Insufficient stock. Cannot reduce stock below zero." });
            }

            // Update stock using repository method
            var updated = await _repository.UpdateStockAsync(
                request.InventoryId,
                request.Quantity,
                request.TransactionType,
                cancellationToken
            );

            var dto = _mapper.Map<InventoryDto>(updated);
            return Result<InventoryDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<InventoryDto>.Failure(new[] { $"Failed to adjust stock: {ex.Message}" });
        }
    }
}

