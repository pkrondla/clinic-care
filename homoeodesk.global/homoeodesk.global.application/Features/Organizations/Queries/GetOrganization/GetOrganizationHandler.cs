using AutoMapper;
using HomoeoDesk.Global.Application.Common;
using HomoeoDesk.Global.Application.Common.Interfaces.Global;
using HomoeoDesk.Global.Application.Common.Models;
using HomoeoDesk.Global.Application.Features.Organizations.Commands.CreateOrganization;
using MediatR;

namespace HomoeoDesk.Global.Application.Features.Organizations.Queries.GetOrganization;

public class GetOrganizationHandler : IRequestHandler<GetOrganizationQuery, Result<OrganizationDto>>
{
    private readonly IGlobalDbContext _context;
    private readonly IMapper _mapper;

    public GetOrganizationHandler(IGlobalDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<OrganizationDto>> Handle(GetOrganizationQuery request, CancellationToken cancellationToken)
    {
        try
        {
            Domain.Entities.GlobalTenant? tenant;

            if (request.Id.HasValue)
                tenant = await GlobalTenantQueries.GetByIdAsync(_context, request.Id.Value, cancellationToken);
            else if (!string.IsNullOrWhiteSpace(request.Subdomain))
                tenant = await GlobalTenantQueries.GetBySubdomainAsync(_context, request.Subdomain, cancellationToken);
            else
                return Result<OrganizationDto>.Failure(new[] { "Either Id or Subdomain must be provided." });

            if (tenant == null)
                return Result<OrganizationDto>.Failure(new[] { "Organization not found." });

            return Result<OrganizationDto>.Success(_mapper.Map<OrganizationDto>(tenant));
        }
        catch (Exception ex)
        {
            return Result<OrganizationDto>.Failure(new[] { $"Failed to retrieve organization: {ex.Message}" });
        }
    }
}
