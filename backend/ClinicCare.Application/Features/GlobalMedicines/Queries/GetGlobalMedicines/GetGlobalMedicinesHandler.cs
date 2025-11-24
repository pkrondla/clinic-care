using AutoMapper;
using ClinicCare.Application.Common.Interfaces.Global;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.GlobalMedicines.Commands.CreateGlobalMedicine;
using MediatR;

namespace ClinicCare.Application.Features.GlobalMedicines.Queries.GetGlobalMedicines;

public class GetGlobalMedicinesHandler : IRequestHandler<GetGlobalMedicinesQuery, Result<List<GlobalMedicineDto>>>
{
    private readonly IGlobalMedicineRepository _repository;
    private readonly IMapper _mapper;

    public GetGlobalMedicinesHandler(IGlobalMedicineRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<List<GlobalMedicineDto>>> Handle(GetGlobalMedicinesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            List<Domain.Entities.GlobalMedicine> medicines;

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                medicines = await _repository.SearchAsync(request.SearchTerm, cancellationToken);
            }
            else if (!string.IsNullOrWhiteSpace(request.Type))
            {
                medicines = await _repository.GetByTypeAsync(request.Type, cancellationToken);
            }
            else if (!string.IsNullOrWhiteSpace(request.Manufacturer))
            {
                medicines = await _repository.GetByManufacturerAsync(request.Manufacturer, cancellationToken);
            }
            else
            {
                medicines = await _repository.GetAllAsync(cancellationToken);
            }

            var dtos = _mapper.Map<List<GlobalMedicineDto>>(medicines);

            return Result<List<GlobalMedicineDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<GlobalMedicineDto>>.Failure(new[] { $"Failed to retrieve global medicines: {ex.Message}" });
        }
    }
}

