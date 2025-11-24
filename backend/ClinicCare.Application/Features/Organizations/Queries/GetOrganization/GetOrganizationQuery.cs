using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Organizations.Commands.CreateOrganization;
using MediatR;

namespace ClinicCare.Application.Features.Organizations.Queries.GetOrganization;

public class GetOrganizationQuery : IRequest<Result<OrganizationDto>>
{
    public int? Id { get; set; }
    public string? Subdomain { get; set; }
}

