using AutoMapper;
using HomoeoDesk.Global.Application.Common;
using HomoeoDesk.Global.Application.Common.Interfaces.Global;
using HomoeoDesk.Global.Application.Common.Models;
using HomoeoDesk.Global.Application.Features.Organizations.Commands.CreateOrganization;
using MediatR;

namespace HomoeoDesk.Global.Application.Features.Organizations.Commands.UpdateOrganization;

public class UpdateOrganizationHandler : IRequestHandler<UpdateOrganizationCommand, Result<OrganizationDto>>
{
    private readonly IGlobalDbContext _context;
    private readonly IMapper _mapper;

    public UpdateOrganizationHandler(IGlobalDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<OrganizationDto>> Handle(UpdateOrganizationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenant = await GlobalTenantQueries.GetByIdAsync(_context, request.Id, cancellationToken);
            if (tenant == null)
                return Result<OrganizationDto>.Failure(new[] { $"Organization with ID {request.Id} not found." });

            tenant.Name = request.Name;
            tenant.ContactEmail = request.ContactEmail;
            tenant.ContactPhone = request.ContactPhone ?? string.Empty;
            tenant.Address = request.Address ?? string.Empty;
            tenant.UpdatedAt = DateTime.UtcNow;

            if (request.IsActive.HasValue)
                tenant.IsActive = request.IsActive.Value;

            await _context.SaveChangesAsync(cancellationToken);

            return Result<OrganizationDto>.Success(_mapper.Map<OrganizationDto>(tenant));
        }
        catch (Exception ex)
        {
            return Result<OrganizationDto>.Failure(new[] { $"Failed to update organization: {ex.Message}" });
        }
    }
}
