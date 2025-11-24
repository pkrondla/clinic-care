using AutoMapper;
using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Interfaces.Tenant;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Inventory.Commands.AdjustStock;
using ClinicCare.Domain.Entities;
using MediatR;

namespace ClinicCare.Application.Features.Inventory.Commands.CreateInventoryItem;

public class CreateInventoryItemHandler : IRequestHandler<CreateInventoryItemCommand, Result<InventoryDto>>
{
    private readonly IInventoryRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public CreateInventoryItemHandler(
        IInventoryRepository repository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<Result<InventoryDto>> Handle(CreateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var clinicId = request.ClinicId;
            if (clinicId == 0)
            {
                return Result<InventoryDto>.Failure(new[] { "Clinic ID is required." });
            }

            // Check if inventory item already exists for this medicine in this clinic
            var existing = await _repository.GetByClinicAndMedicineAsync(clinicId, request.MedicineId, cancellationToken);
            if (existing != null)
            {
                return Result<InventoryDto>.Failure(new[] { "Inventory item for this medicine already exists in this clinic." });
            }

            // Create inventory item
            var inventory = new Domain.Entities.Inventory
            {
                ClinicId = clinicId,
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

            var created = await _repository.AddAsync(inventory, cancellationToken);
            var dto = _mapper.Map<InventoryDto>(created);

            return Result<InventoryDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<InventoryDto>.Failure(new[] { $"Failed to create inventory item: {ex.Message}" });
        }
    }
}

