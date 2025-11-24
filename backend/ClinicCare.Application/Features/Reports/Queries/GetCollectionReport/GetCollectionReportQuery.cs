using ClinicCare.Application.Common.Models;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace ClinicCare.Application.Features.Reports.Queries.GetCollectionReport;

public class GetCollectionReportQuery : IRequest<Result<CollectionReportDto>>
{
    public int? ClinicId { get; set; }
    public int? DoctorId { get; set; }
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    public string? GroupBy { get; set; } // "day", "week", "month", "clinic", "doctor", "paymentMethod"
}

public class CollectionReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalCollection { get; set; }
    public decimal TotalPending { get; set; }
    public int TotalInvoices { get; set; }
    public int PaidInvoices { get; set; }
    public int PendingInvoices { get; set; }
    public List<CollectionReportItemDto> Items { get; set; } = new();
    public List<PaymentMethodBreakdownDto> PaymentMethodBreakdown { get; set; } = new();
    public List<DailyCollectionDto> DailyCollections { get; set; } = new();
}

public class CollectionReportItemDto
{
    public string GroupKey { get; set; } = string.Empty; // Date, Clinic Name, Doctor Name, etc.
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    public int InvoiceCount { get; set; }
}

public class PaymentMethodBreakdownDto
{
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

public class DailyCollectionDto
{
    public DateTime Date { get; set; }
    public decimal Collection { get; set; }
    public int InvoiceCount { get; set; }
}

