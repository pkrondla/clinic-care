using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.ClinicMedicines.Commands.CreateClinicMedicine;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace HomoeoDesk.Tenant.Application.Features.ClinicMedicines.Commands.AddClinicMedicineFromGlobal;

public class AddClinicMedicineFromGlobalCommand : IRequest<Result<ClinicMedicineDto>>
{
    [Required]
    public int GlobalMedicineId { get; set; }

    [Required]
    public int BranchId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? PurchasePrice { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? SellingPrice { get; set; }
}
