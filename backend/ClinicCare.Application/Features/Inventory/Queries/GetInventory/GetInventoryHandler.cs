using AutoMapper;
using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Interfaces.Tenant;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Inventory.Commands.AdjustStock;
using MediatR;

namespace ClinicCare.Application.Features.Inventory.Queries.GetInventory;

public class GetInventoryHandler : IRequestHandler<GetInventoryQuery, Result<List<InventoryDto>>>
{
    private readonly IInventoryRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetInventoryHandler(
        IInventoryRepository repository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<Result<List<InventoryDto>>> Handle(GetInventoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var clinicId = request.ClinicId ?? 0;
            if (clinicId == 0)
            {
                return Result<List<InventoryDto>>.Failure(new[] { "Clinic ID is required." });
            }

            var inventory = await _repository.GetByClinicIdAsync(clinicId, cancellationToken);
            var dtos = _mapper.Map<List<InventoryDto>>(inventory);

            return Result<List<InventoryDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<InventoryDto>>.Failure(new[] { $"Failed to retrieve inventory: {ex.Message}" });
        }
    }
}

