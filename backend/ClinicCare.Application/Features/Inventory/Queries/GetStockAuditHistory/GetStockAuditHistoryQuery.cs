using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Inventory.Queries.GetStockAuditHistory;
using MediatR;

namespace ClinicCare.Application.Features.Inventory.Queries.GetStockAuditHistory;

public class GetStockAuditHistoryQuery : IRequest<Result<List<StockAuditHistoryDto>>>
{
    public int? ClinicId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class StockAuditHistoryDto
{
    public int Id { get; set; }
    public int ClinicId { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public int SystemStock { get; set; }
    public int PhysicalStock { get; set; }
    public int Variance { get; set; }
    public DateTime AuditDate { get; set; }
    public int? AuditedByUserId { get; set; }
    public string? AuditedByUserName { get; set; }
    public string? Notes { get; set; }
    public string Reference { get; set; } = string.Empty;
}

