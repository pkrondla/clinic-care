using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Invoices.Commands.CreateInvoiceFromPrescription;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace ClinicCare.Application.Features.Invoices.Commands.CreateInvoice;

public class CreateInvoiceCommand : IRequest<Result<InvoiceDto>>
{
    [Required]
    public int ClinicId { get; set; }

    [Required]
    public int PatientId { get; set; }

    public int? ConsultationId { get; set; }
    public int? PrescriptionId { get; set; }

    public decimal ConsultationAmount { get; set; }
    public decimal MedicineAmount { get; set; }
    public decimal CourierCharges { get; set; }

    [Required]
    public List<InvoiceItemCommand> Items { get; set; } = new();

    public DateTime? InvoiceDate { get; set; }
}

public class InvoiceItemCommand
{
    [Required]
    public string ItemType { get; set; } = string.Empty; // Consultation, Medicine, Courier, Other

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public int Quantity { get; set; }

    [Required]
    public decimal UnitPrice { get; set; }

    // Optional: Medicine ID for stock reduction
    public int? MedicineId { get; set; }
}

