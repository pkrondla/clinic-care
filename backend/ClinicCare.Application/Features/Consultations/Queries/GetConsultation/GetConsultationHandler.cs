using AutoMapper;
using ClinicCare.Application.Common.Interfaces.Tenant;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Consultations.Commands.CreateConsultation;
using MediatR;

namespace ClinicCare.Application.Features.Consultations.Queries.GetConsultation;

public class GetConsultationHandler : IRequestHandler<GetConsultationQuery, Result<ConsultationDto>>
{
    private readonly IConsultationRepository _repository;
    private readonly IMapper _mapper;

    public GetConsultationHandler(IConsultationRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<ConsultationDto>> Handle(GetConsultationQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var consultation = await _repository.GetByIdWithDetailsAsync(request.Id, cancellationToken);
            if (consultation == null)
            {
                return Result<ConsultationDto>.Failure(new[] { "Consultation not found." });
            }

            var dto = _mapper.Map<ConsultationDto>(consultation);
            return Result<ConsultationDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<ConsultationDto>.Failure(new[] { $"Failed to retrieve consultation: {ex.Message}" });
        }
    }
}

