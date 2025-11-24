using AutoMapper;
using ClinicCare.Application.Common.Interfaces.Tenant;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Consultations.Commands.CreateConsultation;
using MediatR;

namespace ClinicCare.Application.Features.Consultations.Queries.GetPatientConsultations;

public class GetPatientConsultationsHandler : IRequestHandler<GetPatientConsultationsQuery, Result<List<ConsultationDto>>>
{
    private readonly IConsultationRepository _repository;
    private readonly IMapper _mapper;

    public GetPatientConsultationsHandler(IConsultationRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<List<ConsultationDto>>> Handle(GetPatientConsultationsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var consultations = await _repository.GetByPatientIdAsync(request.PatientId, cancellationToken);
            var dtos = _mapper.Map<List<ConsultationDto>>(consultations);

            return Result<List<ConsultationDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<ConsultationDto>>.Failure(new[] { $"Failed to retrieve patient consultations: {ex.Message}" });
        }
    }
}

