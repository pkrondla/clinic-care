using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Clinics.Commands.CreateClinic;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace ClinicCare.Application.Features.Clinics.Commands.UpdateClinic;

public class UpdateClinicCommand : IRequest<Result<ClinicDto>>
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [EmailAddress]
    [MaxLength(255)]
    public string? Email { get; set; }

    public bool? IsActive { get; set; }
}

