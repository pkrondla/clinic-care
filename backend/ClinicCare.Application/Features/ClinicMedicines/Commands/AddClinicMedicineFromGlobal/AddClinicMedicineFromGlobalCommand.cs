using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.ClinicMedicines.Commands.CreateClinicMedicine;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace ClinicCare.Application.Features.ClinicMedicines.Commands.AddClinicMedicineFromGlobal;

public class AddClinicMedicineFromGlobalCommand : IRequest<Result<ClinicMedicineDto>>
{
    [Required]
    public int GlobalMedicineId { get; set; }

    [Required]
    public int ClinicId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? PurchasePrice { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? SellingPrice { get; set; }
}

