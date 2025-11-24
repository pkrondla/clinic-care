using AutoMapper;
using ClinicCare.Application.Common.Interfaces.Global;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Organizations.Commands.CreateOrganization;
using MediatR;

namespace ClinicCare.Application.Features.Organizations.Queries.GetOrganizations;

public class GetOrganizationsHandler : IRequestHandler<GetOrganizationsQuery, Result<List<OrganizationDto>>>
{
    private readonly IOrganizationRepository _repository;
    private readonly IMapper _mapper;

    public GetOrganizationsHandler(IOrganizationRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<List<OrganizationDto>>> Handle(GetOrganizationsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var organizations = await _repository.GetAllAsync(cancellationToken);
            var dtos = _mapper.Map<List<OrganizationDto>>(organizations);

            return Result<List<OrganizationDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<OrganizationDto>>.Failure(new[] { $"Failed to retrieve organizations: {ex.Message}" });
        }
    }
}

