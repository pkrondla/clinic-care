using AutoMapper;
using HomoeoDesk.Global.Application.Common;
using HomoeoDesk.Global.Application.Common.Interfaces.Global;
using HomoeoDesk.Global.Application.Common.Models;
using HomoeoDesk.Global.Application.Features.Organizations.Commands.CreateOrganization;
using HomoeoDesk.Global.Domain.Entities;
using HomoeoDesk.Global.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Global.Application.Features.Organizations.Commands.CreateOrganization;

public class CreateOrganizationHandler : IRequestHandler<CreateOrganizationCommand, Result<OrganizationDto>>
{
    private readonly IGlobalDbContext _context;
    private readonly IMapper _mapper;

    public CreateOrganizationHandler(IGlobalDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<OrganizationDto>> Handle(CreateOrganizationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            string subdomain;
            if (!string.IsNullOrWhiteSpace(request.Subdomain))
            {
                if (await GlobalTenantQueries.SubdomainExistsAsync(_context, request.Subdomain, null, cancellationToken))
                    return Result<OrganizationDto>.Failure(new[] { $"Subdomain '{request.Subdomain}' is already taken." });

                subdomain = request.Subdomain.ToLower();
            }
            else
            {
                subdomain = await GlobalTenantQueries.GenerateSubdomainAsync(_context, request.Name, cancellationToken);
            }

            var tenant = new GlobalTenant
            {
                Name = request.Name,
                Subdomain = subdomain,
                DatabaseName = $"HomoeoDesk_{subdomain}",
                ContactEmail = request.ContactEmail,
                ContactPhone = request.ContactPhone ?? string.Empty,
                Address = request.Address ?? string.Empty,
                SubscriptionStatus = SubscriptionStatus.Trial,
                TrialEndDate = DateTime.UtcNow.AddDays(30),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.GlobalTenants.Add(tenant);
            await _context.SaveChangesAsync(cancellationToken);

            var dto = _mapper.Map<OrganizationDto>(tenant);
            return Result<OrganizationDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<OrganizationDto>.Failure(new[] { $"Failed to create organization: {ex.Message}" });
        }
    }
}
