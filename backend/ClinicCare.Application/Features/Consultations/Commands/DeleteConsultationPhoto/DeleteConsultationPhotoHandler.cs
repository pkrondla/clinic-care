using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.Consultations.Commands.DeleteConsultationPhoto;

public class DeleteConsultationPhotoHandler : IRequestHandler<DeleteConsultationPhotoCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public DeleteConsultationPhotoHandler(IApplicationDbContext context, ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<Result<bool>> Handle(DeleteConsultationPhotoCommand request, CancellationToken cancellationToken)
    {
        var organizationId = await _tenantService.GetOrganizationIdAsync();

        var photo = await _context.ConsultationPhotos
            .FirstOrDefaultAsync(p => p.Id == request.PhotoId && p.OrganizationId == organizationId, cancellationToken);

        if (photo == null)
        {
            return Result<bool>.Failure("Photo not found");
        }

        _context.ConsultationPhotos.Remove(photo);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}

