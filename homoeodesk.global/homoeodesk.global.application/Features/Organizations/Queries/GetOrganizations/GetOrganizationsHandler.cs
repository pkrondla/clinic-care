using AutoMapper;
using HomoeoDesk.Global.Application.Common.Interfaces.Global;
using HomoeoDesk.Global.Application.Common.Models;
using HomoeoDesk.Global.Application.Features.Organizations.Commands.CreateOrganization;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Global.Application.Features.Organizations.Queries.GetOrganizations;

public class GetOrganizationsHandler : IRequestHandler<GetOrganizationsQuery, Result<List<OrganizationDto>>>
{
    private readonly IGlobalDbContext _context;
    private readonly IMapper _mapper;

    public GetOrganizationsHandler(IGlobalDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<List<OrganizationDto>>> Handle(GetOrganizationsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tenants = await _context.GlobalTenants
                .Include(o => o.Subscriptions)
                .Where(o => o.IsActive)
                .OrderBy(o => o.Name)
                .ToListAsync(cancellationToken);

            return Result<List<OrganizationDto>>.Success(_mapper.Map<List<OrganizationDto>>(tenants));
        }
        catch (Exception ex)
        {
            return Result<List<OrganizationDto>>.Failure(new[] { $"Failed to retrieve organizations: {ex.Message}" });
        }
    }
}
