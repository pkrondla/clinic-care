using AutoMapper;
using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Consultations.Commands.AddConsultationPhoto;
using HomoeoDesk.Tenant.Application.Features.Consultations.Commands.CreateConsultation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Consultations.Queries.GetPatientConsultations;

public class GetPatientConsultationsHandler : IRequestHandler<GetPatientConsultationsQuery, Result<List<ConsultationDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPatientConsultationsHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<List<ConsultationDto>>> Handle(GetPatientConsultationsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var consultations = await _context.Consultations
                .Include(c => c.Patient)
                    .ThenInclude(p => p!.User)
                .Include(c => c.Doctor)
                    .ThenInclude(d => d!.User)
                .Include(c => c.Appointment)
                .Include(c => c.Photos)
                .Where(c => c.PatientId == request.PatientId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(cancellationToken);

            var dtos = consultations.Select(c =>
            {
                var dto = _mapper.Map<ConsultationDto>(c);
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
