using ClinicCare.Application.Common.Models;
using MediatR;

namespace ClinicCare.Application.Features.Invoices.Queries.PrepareInvoiceFromPrescription;

public record PrepareInvoiceFromPrescriptionQuery(int PrescriptionId) 
    : IRequest<Result<InvoicePreparationDto>>;

public class InvoicePreparationDto
{
    public int ClinicId { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string PatientCode { get; set; } = string.Empty;
    public int ConsultationId { get; set; }
    public int PrescriptionId { get; set; }
    public decimal ConsultationAmount { get; set; }
    public decimal MedicineAmount { get; set; }
    public decimal CourierCharges { get; set; }
    public List<InvoiceItemPreparationDto> Items { get; set; } = new();
}

public class InvoiceItemPreparationDto
{
    public string ItemType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

