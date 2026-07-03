using HomoeoDesk.Global.Application.Common.Models;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace HomoeoDesk.Global.Application.Features.Organizations.Commands.CreateOrganization;

public class CreateOrganizationCommand : IRequest<Result<OrganizationDto>>
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Subdomain { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string ContactEmail { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? ContactPhone { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    public bool CreateDatabase { get; set; } = true;
}

public class OrganizationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public string SubscriptionStatus { get; set; } = string.Empty;
    public DateTime? TrialEndDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
