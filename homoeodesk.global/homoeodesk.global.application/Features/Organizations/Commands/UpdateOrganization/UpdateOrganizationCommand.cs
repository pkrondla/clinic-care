using HomoeoDesk.Global.Application.Common.Models;
using HomoeoDesk.Global.Application.Features.Organizations.Commands.CreateOrganization;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace HomoeoDesk.Global.Application.Features.Organizations.Commands.UpdateOrganization;

public class UpdateOrganizationCommand : IRequest<Result<OrganizationDto>>
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string ContactEmail { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? ContactPhone { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    public bool? IsActive { get; set; }
}
