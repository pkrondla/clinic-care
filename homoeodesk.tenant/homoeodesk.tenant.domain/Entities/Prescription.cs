using HomoeoDesk.Tenant.Domain.Common;
using HomoeoDesk.Tenant.Domain.Enums;

namespace HomoeoDesk.Tenant.Domain.Entities;

public class Prescription : TenantEntity
{
    public int ConsultationId { get; set; }
    public string PrescriptionNumber { get; set; } = string.Empty;
    public PrescriptionStatus Status { get; set; }
    public string InternalNotes { get; set; } = string.Empty;
    public string PatientInstructions { get; set; } = string.Empty;
    public DateTime IssuedDate { get; set; }

    // Navigation Properties
    public Consultation Consultation { get; set; } = null!;
    public ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();
}

public class PrescriptionItem : TenantEntity
{
    public int PrescriptionId { get; set; }
    public int? MedicineId { get; set; } // Nullable to allow custom medicine names
    public string MedicineName { get; set; } = string.Empty;
    public DispensingForm DispensingForm { get; set; }
    public string Dosage { get; set; } = string.Empty; // Auto-set based on DispensingForm
    public string Frequency { get; set; } = string.Empty; // e.g., "Daily 3 times", "Weekly once"
    public string Duration { get; set; } = string.Empty; // e.g., "4 weeks"
    public string Timing { get; set; } = string.Empty; // e.g., "Before food", "Before brushing"
    public int? ContainerSize { get; set; } // Only for Globules: 1, 2, or 3 dram
    public int? Quantity { get; set; } // Required for all forms (prescribed quantity for patient)
    public decimal DispensedQuantity { get; set; } // Internal: quantity dispensed from inventory (in drops for liquids/globules, same as quantity for tablets/packets)
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string Instructions { get; set; } = string.Empty;

    // Navigation Properties
    public Prescription Prescription { get; set; } = null!;
    // Medicine navigation property removed - we store MedicineId and MedicineName directly
    // This prevents EF Core from creating shadow properties like ClinicMedicineId1
}
