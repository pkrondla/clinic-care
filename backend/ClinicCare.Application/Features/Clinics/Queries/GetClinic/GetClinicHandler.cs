using AutoMapper;
using ClinicCare.Application.Common.Interfaces.Tenant;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Clinics.Commands.CreateClinic;
using MediatR;

namespace ClinicCare.Application.Features.Clinics.Queries.GetClinic;

public class GetClinicHandler : IRequestHandler<GetClinicQuery, Result<ClinicDto>>
{
    private readonly IClinicRepository _repository;
    private readonly IMapper _mapper;

    public GetClinicHandler(IClinicRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<ClinicDto>> Handle(GetClinicQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var clinic = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (clinic == null)
            {
                return Result<ClinicDto>.Failure(new[] { "Clinic not found." });
            }

            var dto = _mapper.Map<ClinicDto>(clinic);
            return Result<ClinicDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<ClinicDto>.Failure(new[] { $"Failed to retrieve clinic: {ex.Message}" });
        }
    }
}

