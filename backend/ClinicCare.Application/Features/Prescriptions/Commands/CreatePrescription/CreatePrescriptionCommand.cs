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
    public int? MedicineId { get; set; } // ClinicMedicineId (nullable to allow custom medicine names)
    public string MedicineName { get; set; } = string.Empty;
    public int DispensingForm { get; set; } // 1 = Globules, 2 = Tablets, 3 = Packet
    public string Dosage { get; set; } = string.Empty; // Auto-set based on DispensingForm
    public string Frequency { get; set; } = string.Empty; // e.g., "Daily 3 times", "Weekly once"
    public string Duration { get; set; } = string.Empty; // e.g., "4 weeks"
    public string Timing { get; set; } = string.Empty; // e.g., "Before food", "Before brushing"
    public int? ContainerSize { get; set; } // Only for Globules: 1, 2, or 3 dram
    public int? Quantity { get; set; } // Required for all forms (prescribed quantity for patient)
    public decimal? DispensedQuantity { get; set; } // Internal: quantity dispensed from inventory (auto-calculated, not shown to patient)
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
    public bool HasInvoice { get; set; }
    public int? InvoiceId { get; set; }
}

