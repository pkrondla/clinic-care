using ClinicCare.Application.Common.Models;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace ClinicCare.Application.Features.ClinicMedicines.Commands.DeleteClinicMedicine;

public class DeleteClinicMedicineCommand : IRequest<Result<bool>>
{
    [Required]
    public int Id { get; set; }
}

