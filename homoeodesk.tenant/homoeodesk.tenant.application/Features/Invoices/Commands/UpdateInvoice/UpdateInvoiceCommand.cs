using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Invoices.Commands.CreateInvoiceFromPrescription;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace HomoeoDesk.Tenant.Application.Features.Invoices.Commands.UpdateInvoice;

public class UpdateInvoiceCommand : IRequest<Result<InvoiceDto>>
{
    [Required]
    public int Id { get; set; }

    public int? BranchId { get; set; }
    public int? PatientId { get; set; }

    public decimal? ConsultationAmount { get; set; }
    public decimal? MedicineAmount { get; set; }
    public decimal? CourierCharges { get; set; }

    public List<InvoiceItemCommand>? Items { get; set; }

    public DateTime? InvoiceDate { get; set; }
    public int? Status { get; set; }
    public int? CourierStatus { get; set; }
}

public class InvoiceItemCommand
{
    public int? Id { get; set; } // For existing items
    public string ItemType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

