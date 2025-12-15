using AutoMapper;
using ClinicCare.Application.Common.Interfaces.Tenant;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Consultations.Commands.AddConsultationPhoto;
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
            var dtos = consultations.Select(c => 
            {
                var dto = _mapper.Map<ConsultationDto>(c);
                // Set photos
                dto.Photos = c.Photos?
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.DisplayOrder)
                    .Select(p => new ConsultationPhotoDto
                    {
                        Id = p.Id,
                        ConsultationId = p.ConsultationId,
                        PhotoUrl = p.PhotoUrl,
                        Description = p.Description,
                        DisplayOrder = p.DisplayOrder,
                        CreatedAt = p.CreatedAt
                    })
                    .ToList() ?? new List<ConsultationPhotoDto>();
                return dto;
            }).ToList();

            return Result<List<ConsultationDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<ConsultationDto>>.Failure(new[] { $"Failed to retrieve patient consultations: {ex.Message}" });
        }
    }
}

