using HomoeoDesk.Tenant.Application.Common.Models;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace HomoeoDesk.Tenant.Application.Features.Reports.Queries.GetPatientReport;

public class GetPatientReportQuery : IRequest<Result<PatientReportDto>>
{
    public int? PatientId { get; set; }
    public int? BranchId { get; set; }
    public int? DoctorId { get; set; }
    
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class PatientReportDto
{
    public int? PatientId { get; set; }
    public string? PatientName { get; set; }
    public string? PatientCode { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    // Summary Statistics
    public int TotalVisits { get; set; }
    public int TotalConsultations { get; set; }
    public int TotalPrescriptions { get; set; }
    public int TotalInvoices { get; set; }
    public decimal TotalAmountPaid { get; set; }
    public decimal TotalAmountPending { get; set; }
    
    // Visit History
    public List<PatientVisitDto> VisitHistory { get; set; } = new();
    
    // Treatment Summary
    public List<TreatmentSummaryDto> TreatmentSummary { get; set; } = new();
    
    // Medication History
    public List<MedicationHistoryDto> MedicationHistory { get; set; } = new();
    
    // Payment History
    public List<PaymentHistoryDto> PaymentHistory { get; set; } = new();
}

public class PatientVisitDto
{
    public DateTime VisitDate { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string AppointmentType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? ConsultationId { get; set; }
    public int? PrescriptionId { get; set; }
    public int? InvoiceId { get; set; }
}

public class TreatmentSummaryDto
{
    public DateTime ConsultationDate { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string TreatmentPlan { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public class MedicationHistoryDto
{
    public DateTime PrescriptionDate { get; set; }
    public string PrescriptionNumber { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public int MedicineCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<MedicationItemDto> Medications { get; set; } = new();
}

public class MedicationItemDto
{
    public string MedicineName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public int Duration { get; set; }
    public string DurationUnit { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
}

public class PaymentHistoryDto
{
    public DateTime InvoiceDate { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? PaymentDate { get; set; }
}

