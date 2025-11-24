using ClinicCare.Application.Common.Models;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace ClinicCare.Application.Features.Prescriptions.Commands.CreatePrescription;

public class CreatePrescriptionCommand : IRequest<Result<PrescriptionDto>>
{
    public int ConsultationId { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }

    [Required]
    public List<PrescriptionMedicineDto> Medicines { get; set; } = new();

    public string? Notes { get; set; }
}

public class PrescriptionMedicineDto
{
    public int MedicineId { get; set; } // ClinicMedicineId
    public string MedicineName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public int Duration { get; set; } // in days
    public int Quantity { get; set; }
    public string? Instructions { get; set; }
}

public class PrescriptionDto
{
    public int Id { get; set; }
    public string PrescriptionNumber { get; set; } = string.Empty;
    public int ConsultationId { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public DateTime PrescriptionDate { get; set; }
    public List<PrescriptionMedicineDto> Medicines { get; set; } = new();
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

