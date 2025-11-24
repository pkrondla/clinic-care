using AutoMapper;
using ClinicCare.Application.Common.Interfaces.Global;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.GlobalMedicines.Commands.CreateGlobalMedicine;
using MediatR;

namespace ClinicCare.Application.Features.GlobalMedicines.Commands.UpdateGlobalMedicine;

public class UpdateGlobalMedicineHandler : IRequestHandler<UpdateGlobalMedicineCommand, Result<GlobalMedicineDto>>
{
    private readonly IGlobalMedicineRepository _repository;
    private readonly IMapper _mapper;

    public UpdateGlobalMedicineHandler(IGlobalMedicineRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<GlobalMedicineDto>> Handle(UpdateGlobalMedicineCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get existing medicine
            var medicine = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (medicine == null)
            {
                return Result<GlobalMedicineDto>.Failure(new[] { $"Global medicine with ID {request.Id} not found." });
            }

            // Update properties
            medicine.Name = request.Name;
            medicine.GenericName = request.GenericName;
            medicine.Type = request.Type;
            medicine.Potency = request.Potency;
            medicine.Manufacturer = request.Manufacturer;
            medicine.Price = request.Price;
            medicine.Description = request.Description ?? string.Empty;

            // Save changes
            await _repository.UpdateAsync(medicine, cancellationToken);

            // Map to DTO
            var dto = _mapper.Map<GlobalMedicineDto>(medicine);

            return Result<GlobalMedicineDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<GlobalMedicineDto>.Failure(new[] { $"Failed to update global medicine: {ex.Message}" });
        }
    }
}

