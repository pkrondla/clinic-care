using AutoMapper;
using ClinicCare.Application.Common.Interfaces.Global;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.GlobalMedicines.Commands.CreateGlobalMedicine;
using MediatR;

namespace ClinicCare.Application.Features.GlobalMedicines.Queries.GetGlobalMedicine;

public class GetGlobalMedicineHandler : IRequestHandler<GetGlobalMedicineQuery, Result<GlobalMedicineDto>>
{
    private readonly IGlobalMedicineRepository _repository;
    private readonly IMapper _mapper;

    public GetGlobalMedicineHandler(IGlobalMedicineRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<GlobalMedicineDto>> Handle(GetGlobalMedicineQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var medicine = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (medicine == null)
            {
                return Result<GlobalMedicineDto>.Failure(new[] { $"Global medicine with ID {request.Id} not found." });
            }

            var dto = _mapper.Map<GlobalMedicineDto>(medicine);

            return Result<GlobalMedicineDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<GlobalMedicineDto>.Failure(new[] { $"Failed to retrieve global medicine: {ex.Message}" });
        }
    }
}

