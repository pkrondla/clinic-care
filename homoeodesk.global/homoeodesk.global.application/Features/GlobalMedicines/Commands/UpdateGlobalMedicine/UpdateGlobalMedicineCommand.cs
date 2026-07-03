using HomoeoDesk.Global.Application.Common.Models;
using HomoeoDesk.Global.Application.Features.GlobalMedicines.Commands.CreateGlobalMedicine;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace HomoeoDesk.Global.Application.Features.GlobalMedicines.Commands.UpdateGlobalMedicine;

public class UpdateGlobalMedicineCommand : IRequest<Result<GlobalMedicineDto>>
{
    public int Id { get; set; }

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
