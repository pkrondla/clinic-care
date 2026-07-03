using HomoeoDesk.Global.Application.Common.Models;
using HomoeoDesk.Global.Application.Features.Organizations.Commands.CreateOrganization;
using MediatR;

namespace HomoeoDesk.Global.Application.Features.Organizations.Queries.GetOrganizations;

public class GetOrganizationsQuery : IRequest<Result<List<OrganizationDto>>>
{
}
