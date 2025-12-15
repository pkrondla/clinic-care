using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Invoices.Commands.CreateInvoiceFromPrescription;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace ClinicCare.Application.Features.Invoices.Commands.UpdateInvoice;

public class UpdateInvoiceCommand : IRequest<Result<InvoiceDto>>
{
    [Required]
    public int Id { get; set; }

    public int? ClinicId { get; set; }
    public int? PatientId { get; set; }

    public decimal? ConsultationAmount { get; set; }
    public decimal? MedicineAmount { get; set; }
    public decimal? CourierCharges { get; set; }

    public List<InvoiceItemCommand>? Items { get; set; }

    public DateTime? InvoiceDate { get; set; }
    public int? Status { get; set; }
}

public class InvoiceItemCommand
{
    public int? Id { get; set; } // For existing items
    public string ItemType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

