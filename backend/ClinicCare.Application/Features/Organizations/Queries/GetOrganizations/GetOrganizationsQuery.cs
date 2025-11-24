using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Organizations.Commands.CreateOrganization;
using MediatR;

namespace ClinicCare.Application.Features.Organizations.Queries.GetOrganizations;

public class GetOrganizationsQuery : IRequest<Result<List<OrganizationDto>>>
{
    // Optional filters can be added here
}

