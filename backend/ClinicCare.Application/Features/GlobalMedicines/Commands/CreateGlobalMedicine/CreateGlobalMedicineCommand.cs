using ClinicCare.Application.Common.Models;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace ClinicCare.Application.Features.GlobalMedicines.Commands.CreateGlobalMedicine;

public class CreateGlobalMedicineCommand : IRequest<Result<GlobalMedicineDto>>
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string GenericName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Type { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Potency { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Manufacturer { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }
}

public class GlobalMedicineDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string GenericName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Potency { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

