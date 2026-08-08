using HomoeoDesk.Global.Application.Common.Models;
using HomoeoDesk.Global.Application.Features.Organizations.Commands.CreateOrganization;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace HomoeoDesk.Global.Application.Features.PublicRegistration.Commands.RegisterTrialOrganization;

public class RegisterTrialOrganizationCommand : IRequest<Result<OrganizationDto>>
{
    [Required]
    [MaxLength(200)]
    public string ClinicName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string ContactName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(40)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? Subdomain { get; set; }

    [MaxLength(120)]
    public string? City { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }
}
