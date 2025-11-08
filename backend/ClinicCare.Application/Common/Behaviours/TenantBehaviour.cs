using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Common.Behaviours;

public class TenantBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public TenantBehaviour(IApplicationDbContext context, ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Apply tenant filter to all queries
        if (_tenantService.OrganizationId.HasValue)
        {
            ApplyTenantFilter();
        }

        return await next();
    }

    private void ApplyTenantFilter()
    {
        var organizationId = _tenantService.OrganizationId!.Value;

        // Apply tenant filters to all tenant entities
        _context.Users.Where(x => x.OrganizationId == organizationId);
        _context.Clinics.Where(x => x.OrganizationId == organizationId);
        _context.DoctorProfiles.Where(x => x.OrganizationId == organizationId);
        _context.Patients.Where(x => x.OrganizationId == organizationId);
        _context.Appointments.Where(x => x.OrganizationId == organizationId);
        // Add more as needed
    }
}
