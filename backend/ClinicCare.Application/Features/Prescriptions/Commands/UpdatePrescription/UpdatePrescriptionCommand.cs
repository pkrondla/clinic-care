using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Prescriptions.Commands.CreatePrescription;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace ClinicCare.Application.Features.Prescriptions.Commands.UpdatePrescription;

public class UpdatePrescriptionCommand : IRequest<Result<PrescriptionDto>>
{
    [Required]
    public int Id { get; set; }

    [Required]
    public List<PrescriptionMedicineDto> Medicines { get; set; } = new();

    public string? Notes { get; set; }
}

