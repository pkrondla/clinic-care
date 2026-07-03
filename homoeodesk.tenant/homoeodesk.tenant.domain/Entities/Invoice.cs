using HomoeoDesk.Tenant.Domain.Common;
using HomoeoDesk.Tenant.Domain.Enums;

namespace HomoeoDesk.Tenant.Domain.Entities;

public class Invoice : TenantEntity
{
    public int BranchId { get; set; }
    public int PatientId { get; set; }
    public int? ConsultationId { get; set; }
    public int? PrescriptionId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal ConsultationAmount { get; set; }
    public decimal MedicineAmount { get; set; }
    public decimal CourierCharges { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    public InvoiceStatus Status { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentReference { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    
    // Courier fields (for teleconsultation)
    public string? CourierDocketNumber { get; set; }
    public string? CourierCompany { get; set; }
    public DateTime? CourierDispatchedDate { get; set; }
    public string? CourierTrackingUrl { get; set; }
    public CourierStatus? CourierStatus { get; set; }

    public bool IsFullyPaid => BalanceAmount <= 0;
    public bool IsPartiallyPaid => PaidAmount > 0 && BalanceAmount > 0;

    // Navigation Properties
    public Branch Branch { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public Consultation? Consultation { get; set; }
    public Prescription? Prescription { get; set; }
    public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
}

public class InvoiceItem : TenantEntity
{
    public int InvoiceId { get; set; }
    public string ItemType { get; set; } = string.Empty; // Consultation, Medicine, Courier
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    // Navigation Properties
    public Invoice Invoice { get; set; } = null!;
}
