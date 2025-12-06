using ClinicCare.Domain.Common;
using ClinicCare.Domain.Enums;

namespace ClinicCare.Domain.Entities;

public class Prescription : TenantEntity
{
    public int ConsultationId { get; set; }
    public string PrescriptionNumber { get; set; } = string.Empty;
    public PrescriptionStatus Status { get; set; }
    public string InternalNotes { get; set; } = string.Empty;
    public string PatientInstructions { get; set; } = string.Empty;
    public DateTime IssuedDate { get; set; }

    // Navigation Properties
    public Organization Organization { get; set; } = null!;
    public Consultation Consultation { get; set; } = null!;
    public ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();
}

public class PrescriptionItem : TenantEntity
{
    public int PrescriptionId { get; set; }
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string Instructions { get; set; } = string.Empty;

    // Navigation Properties
    public Organization Organization { get; set; } = null!;
    public Prescription Prescription { get; set; } = null!;
    // Medicine navigation property removed - we store MedicineId and MedicineName directly
    // This prevents EF Core from creating shadow properties like ClinicMedicineId1
}
