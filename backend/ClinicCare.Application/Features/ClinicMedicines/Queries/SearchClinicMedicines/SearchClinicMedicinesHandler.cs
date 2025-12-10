using AutoMapper;
using ClinicCare.Application.Common.Interfaces.Tenant;
using ClinicCare.Application.Common.Models;
using MediatR;

namespace ClinicCare.Application.Features.ClinicMedicines.Queries.SearchClinicMedicines;

public class SearchClinicMedicinesHandler : IRequestHandler<SearchClinicMedicinesQuery, Result<List<ClinicMedicineSearchDto>>>
{
    private readonly IClinicMedicineRepository _repository;
    private readonly IMapper _mapper;

    public SearchClinicMedicinesHandler(IClinicMedicineRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<List<ClinicMedicineSearchDto>>> Handle(SearchClinicMedicinesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            List<Domain.Entities.ClinicMedicine> medicines;

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                medicines = await _repository.SearchAsync(request.SearchTerm, cancellationToken);
            }
            else
            {
                medicines = await _repository.GetAllAsync(cancellationToken);
            }

            var dtos = medicines.Select(m => new ClinicMedicineSearchDto
            {
                Id = m.Id,
                Name = m.Name,
                GenericName = m.GenericName ?? string.Empty,
                Manufacturer = m.Manufacturer ?? string.Empty,
                Type = m.Type ?? string.Empty,
                Potency = m.Potency ?? string.Empty
            }).ToList();

            return Result<List<ClinicMedicineSearchDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<ClinicMedicineSearchDto>>.Failure(new[] { $"Failed to search clinic medicines: {ex.Message}" });
        }
    }
}

