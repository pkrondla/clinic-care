using HomoeoDesk.Global.Application.Common.Models;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace HomoeoDesk.Global.Application.Features.TrialRequests.Commands.CreateTrialRequest;

public class CreateTrialRequestCommand : IRequest<Result<TrialRequestDto>>
{
    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(40)]
    public string? Phone { get; set; }

    [Required]
    [MaxLength(200)]
    public string ClinicName { get; set; } = string.Empty;

    [MaxLength(120)]
    public string? City { get; set; }

    [MaxLength(40)]
    public string? PatientsPerWeek { get; set; }

    [MaxLength(2000)]
    public string? Message { get; set; }

    [MaxLength(100)]
    public string? Source { get; set; }
}

public class TrialRequestDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? PatientsPerWeek { get; set; }
    public string? Message { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
