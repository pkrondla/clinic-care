using AutoMapper;
using ClinicCare.Application.Common.Interfaces.Global;
using ClinicCare.Application.Common.Models;
using ClinicCare.Domain.Entities;
using ClinicCare.Domain.Enums;
using MediatR;

namespace ClinicCare.Application.Features.Organizations.Commands.CreateOrganization;

public class CreateOrganizationHandler : IRequestHandler<CreateOrganizationCommand, Result<OrganizationDto>>
{
    private readonly IOrganizationRepository _repository;
    private readonly IMapper _mapper;

    public CreateOrganizationHandler(IOrganizationRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<OrganizationDto>> Handle(CreateOrganizationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Generate or validate subdomain
            string subdomain;
            if (!string.IsNullOrWhiteSpace(request.Subdomain))
            {
                // Validate custom subdomain
                if (await _repository.SubdomainExistsAsync(request.Subdomain, null, cancellationToken))
                {
                    return Result<OrganizationDto>.Failure(new[] { $"Subdomain '{request.Subdomain}' is already taken." });
                }
                subdomain = request.Subdomain.ToLower();
            }
            else
            {
                // Auto-generate subdomain from organization name
                subdomain = await _repository.GenerateSubdomainAsync(request.Name, cancellationToken);
            }

            // Create organization entity
            var organization = new Organization
            {
                Name = request.Name,
                Subdomain = subdomain,
                DatabaseName = $"ClinicCare_{subdomain}",
                ContactEmail = request.ContactEmail,
                ContactPhone = request.ContactPhone ?? string.Empty,
                Address = request.Address ?? string.Empty,
                SubscriptionStatus = SubscriptionStatus.Trial,
                TrialEndDate = DateTime.UtcNow.AddDays(30), // 30-day trial
                IsActive = true
            };

            // Save to database
            var created = await _repository.AddAsync(organization, cancellationToken);

            // TODO: Create tenant database if requested
            if (request.CreateDatabase)
            {
                // This would call a database provisioning service
                // await _databaseProvisioningService.CreateTenantDatabaseAsync(created.DatabaseName);
            }

            // Map to DTO
            var dto = _mapper.Map<OrganizationDto>(created);

            return Result<OrganizationDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<OrganizationDto>.Failure(new[] { $"Failed to create organization: {ex.Message}" });
        }
    }
}

