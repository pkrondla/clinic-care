using AutoMapper;
using HomoeoDesk.Global.Application.Common;
using HomoeoDesk.Global.Application.Common.Interfaces.Global;
using HomoeoDesk.Global.Application.Common.Models;
using HomoeoDesk.Global.Application.Features.Organizations.Commands.CreateOrganization;
using HomoeoDesk.Global.Domain.Entities;
using HomoeoDesk.Global.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace HomoeoDesk.Global.Application.Features.PublicRegistration.Commands.RegisterTrialOrganization;

public class RegisterTrialOrganizationHandler : IRequestHandler<RegisterTrialOrganizationCommand, Result<OrganizationDto>>
{
    private static readonly Regex SubdomainRegex = new("^[a-z0-9]([a-z0-9-]{1,38}[a-z0-9])?$", RegexOptions.Compiled);

    private readonly IGlobalDbContext _context;
    private readonly IMapper _mapper;

    public RegisterTrialOrganizationHandler(IGlobalDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<OrganizationDto>> Handle(RegisterTrialOrganizationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            string subdomain;
            if (!string.IsNullOrWhiteSpace(request.Subdomain))
            {
                subdomain = request.Subdomain.Trim().ToLowerInvariant();
                if (!SubdomainRegex.IsMatch(subdomain))
                    return Result<OrganizationDto>.Failure("Subdomain must be 3–40 characters: lowercase letters, numbers, and hyphens.");

                if (await GlobalTenantQueries.SubdomainExistsAsync(_context, subdomain, null, cancellationToken))
                    return Result<OrganizationDto>.Failure($"Subdomain '{subdomain}' is already taken.");
            }
            else
            {
                subdomain = await GlobalTenantQueries.GenerateSubdomainAsync(_context, request.ClinicName, cancellationToken);
            }

            var addressParts = new[] { request.Address?.Trim(), request.City?.Trim() }
                .Where(x => !string.IsNullOrWhiteSpace(x));

            var trialEnd = DateTime.UtcNow.AddDays(30);
            var tenant = new GlobalTenant
            {
                Name = request.ClinicName.Trim(),
                Subdomain = subdomain,
                DatabaseName = $"HomoeoDesk_{subdomain}",
                ContactEmail = request.Email.Trim().ToLowerInvariant(),
                ContactPhone = request.Phone?.Trim() ?? string.Empty,
                Address = string.Join(", ", addressParts),
                SubscriptionStatus = SubscriptionStatus.Trial,
                TrialEndDate = trialEnd,
                IsActive = true
            };

            _context.GlobalTenants.Add(tenant);

            var trialPlan = await _context.SubscriptionPlans
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IsActive && p.Name == "Trial", cancellationToken)
                ?? await _context.SubscriptionPlans
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.IsActive, cancellationToken);

            if (trialPlan is not null)
            {
                _context.OrganizationSubscriptions.Add(new OrganizationSubscription
                {
                    GlobalTenant = tenant,
                    SubscriptionPlanId = trialPlan.Id,
                    StartDate = DateTime.UtcNow,
                    EndDate = trialEnd,
                    Status = SubscriptionStatus.Trial,
                    AutoRenew = false
                });
            }

            _context.TrialRequests.Add(new TrialRequest
            {
                FullName = request.ContactName.Trim(),
                Email = tenant.ContactEmail,
                Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
                ClinicName = tenant.Name,
                City = string.IsNullOrWhiteSpace(request.City) ? null : request.City.Trim(),
                Message = $"Self-serve registration. Preferred subdomain: {subdomain}",
                Source = "website-register",
                Status = TrialRequestStatus.Converted,
                IsActive = true
            });

            await _context.SaveChangesAsync(cancellationToken);

            return Result<OrganizationDto>.Success(_mapper.Map<OrganizationDto>(tenant));
        }
        catch (Exception ex)
        {
            return Result<OrganizationDto>.Failure($"Failed to register trial organization: {ex.Message}");
        }
    }
}
