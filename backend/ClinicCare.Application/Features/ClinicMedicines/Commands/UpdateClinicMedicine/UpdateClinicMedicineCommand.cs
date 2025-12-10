using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.ClinicMedicines.Commands.CreateClinicMedicine;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace ClinicCare.Application.Features.ClinicMedicines.Commands.UpdateClinicMedicine;

public class UpdateClinicMedicineCommand : IRequest<Result<ClinicMedicineDto>>
{
    [Required]
    public int Id { get; set; }

    [MaxLength(200)]
    public string? Name { get; set; }

    [MaxLength(200)]
    public string? GenericName { get; set; }

    [MaxLength(100)]
    public string? Type { get; set; }

    [MaxLength(50)]
    public string? Potency { get; set; }

    [MaxLength(200)]
    public string? Manufacturer { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? PurchasePrice { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? SellingPrice { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool? IsActive { get; set; }
}

