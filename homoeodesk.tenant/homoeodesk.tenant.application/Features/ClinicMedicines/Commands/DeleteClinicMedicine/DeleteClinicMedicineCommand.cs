using HomoeoDesk.Tenant.Application.Common.Models;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace HomoeoDesk.Tenant.Application.Features.ClinicMedicines.Commands.DeleteClinicMedicine;

public class DeleteClinicMedicineCommand : IRequest<Result<bool>>
{
    [Required]
    public int Id { get; set; }
}

