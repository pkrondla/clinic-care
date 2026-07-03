using HomoeoDesk.Global.Application.Common.Models;
using HomoeoDesk.Global.Application.Features.Organizations.Commands.CreateOrganization;
using MediatR;

namespace HomoeoDesk.Global.Application.Features.Organizations.Queries.GetOrganization;

public class GetOrganizationQuery : IRequest<Result<OrganizationDto>>
{
    public int? Id { get; set; }
    public string? Subdomain { get; set; }
}
