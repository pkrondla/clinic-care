using AutoMapper;
using ClinicCare.Application.Common.Interfaces.Global;
using ClinicCare.Application.Common.Models;
using ClinicCare.Domain.Entities;
using MediatR;

namespace ClinicCare.Application.Features.GlobalMedicines.Commands.CreateGlobalMedicine;

public class CreateGlobalMedicineHandler : IRequestHandler<CreateGlobalMedicineCommand, Result<GlobalMedicineDto>>
{
    private readonly IGlobalMedicineRepository _repository;
    private readonly IMapper _mapper;

    public CreateGlobalMedicineHandler(IGlobalMedicineRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<GlobalMedicineDto>> Handle(CreateGlobalMedicineCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if medicine already exists
            var exists = await _repository.ExistsAsync(request.Name, request.Potency, request.Manufacturer, cancellationToken);
            if (exists)
            {
                return Result<GlobalMedicineDto>.Failure(new[] { "A medicine with the same name, potency, and manufacturer already exists." });
            }

            // Create entity
            var medicine = _mapper.Map<GlobalMedicine>(request);

            // Save to database
            var created = await _repository.AddAsync(medicine, cancellationToken);

            // Map to DTO
            var dto = _mapper.Map<GlobalMedicineDto>(created);

            return Result<GlobalMedicineDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<GlobalMedicineDto>.Failure(new[] { $"Failed to create global medicine: {ex.Message}" });
        }
    }
}

