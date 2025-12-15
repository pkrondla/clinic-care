using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Models;
using ClinicCare.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.Consultations.Commands.AddConsultationPhoto;

public class AddConsultationPhotoHandler : IRequestHandler<AddConsultationPhotoCommand, Result<ConsultationPhotoDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public AddConsultationPhotoHandler(IApplicationDbContext context, ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<Result<ConsultationPhotoDto>> Handle(AddConsultationPhotoCommand request, CancellationToken cancellationToken)
    {
        var organizationId = await _tenantService.GetOrganizationIdAsync();

        // Verify consultation exists and belongs to organization
        var consultation = await _context.Consultations
            .FirstOrDefaultAsync(c => c.Id == request.ConsultationId && c.OrganizationId == organizationId, cancellationToken);

        if (consultation == null)
        {
            return Result<ConsultationPhotoDto>.Failure("Consultation not found");
        }

        // Get the next display order
        var existingPhotos = await _context.ConsultationPhotos
            .Where(p => p.ConsultationId == request.ConsultationId)
            .Select(p => p.DisplayOrder)
            .ToListAsync(cancellationToken);

        var maxOrder = existingPhotos.Any() ? existingPhotos.Max() : 0;

        var photo = new ConsultationPhoto
        {
            ConsultationId = request.ConsultationId,
            PhotoUrl = request.PhotoUrl,
            Description = request.Description,
            DisplayOrder = maxOrder + 1,
            OrganizationId = organizationId,
            IsActive = true
        };

        _context.ConsultationPhotos.Add(photo);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new ConsultationPhotoDto
        {
            Id = photo.Id,
            ConsultationId = photo.ConsultationId,
            PhotoUrl = photo.PhotoUrl,
            Description = photo.Description,
            DisplayOrder = photo.DisplayOrder,
            CreatedAt = photo.CreatedAt
        };

        return Result<ConsultationPhotoDto>.Success(dto);
    }
}

