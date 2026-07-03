using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Prescriptions.Commands.CreatePrescription;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace HomoeoDesk.Tenant.Application.Features.Prescriptions.Commands.UpdatePrescription;

public class UpdatePrescriptionCommand : IRequest<Result<PrescriptionDto>>
{
    [Required]
    public int Id { get; set; }

    [Required]
    public List<PrescriptionMedicineDto> Medicines { get; set; } = new();

    public string? Notes { get; set; }
}

