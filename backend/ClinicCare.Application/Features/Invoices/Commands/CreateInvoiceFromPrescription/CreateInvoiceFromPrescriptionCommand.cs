using ClinicCare.Application.Common.Models;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace ClinicCare.Application.Features.Invoices.Commands.CreateInvoiceFromPrescription;

public record CreateInvoiceFromPrescriptionCommand(
    int PrescriptionId,
    decimal? CourierCharges = null
) : IRequest<Result<InvoiceDto>>;

public class InvoiceDto
{
    public int Id { get; set; }
    public int ClinicId { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string PatientCode { get; set; } = string.Empty;
    public int? ConsultationId { get; set; }
    public int? PrescriptionId { get; set; }
    public string PrescriptionNumber { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal ConsultationAmount { get; set; }
    public decimal MedicineAmount { get; set; }
    public decimal CourierCharges { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    public int Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentReference { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? CourierDocketNumber { get; set; }
    public string? CourierCompany { get; set; }
    public DateTime? CourierDispatchedDate { get; set; }
    public string? CourierTrackingUrl { get; set; }
    public int? CourierStatus { get; set; }
    public string? CourierStatusText { get; set; }
    public List<InvoiceItemDto> Items { get; set; } = new();
}

public class InvoiceItemDto
{
    public int Id { get; set; }
    public string ItemType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

