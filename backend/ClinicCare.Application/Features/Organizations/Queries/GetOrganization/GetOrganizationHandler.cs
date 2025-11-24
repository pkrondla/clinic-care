using AutoMapper;
using ClinicCare.Application.Common.Interfaces.Global;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Organizations.Commands.CreateOrganization;
using MediatR;

namespace ClinicCare.Application.Features.Organizations.Queries.GetOrganization;

public class GetOrganizationHandler : IRequestHandler<GetOrganizationQuery, Result<OrganizationDto>>
{
    private readonly IOrganizationRepository _repository;
    private readonly IMapper _mapper;

    public GetOrganizationHandler(IOrganizationRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<OrganizationDto>> Handle(GetOrganizationQuery request, CancellationToken cancellationToken)
    {
        try
        {
            Domain.Entities.Organization? organization = null;

            if (request.Id.HasValue)
            {
                organization = await _repository.GetByIdAsync(request.Id.Value, cancellationToken);
            }
            else if (!string.IsNullOrWhiteSpace(request.Subdomain))
            {
                organization = await _repository.GetBySubdomainAsync(request.Subdomain, cancellationToken);
            }
            else
            {
                return Result<OrganizationDto>.Failure(new[] { "Either Id or Subdomain must be provided." });
            }

            if (organization == null)
            {
                return Result<OrganizationDto>.Failure(new[] { "Organization not found." });
            }

            var dto = _mapper.Map<OrganizationDto>(organization);

            return Result<OrganizationDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<OrganizationDto>.Failure(new[] { $"Failed to retrieve organization: {ex.Message}" });
        }
    }
}

