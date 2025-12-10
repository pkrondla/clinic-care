using ClinicCare.Application.Common.Models;
using MediatR;

namespace ClinicCare.Application.Features.ClinicMedicines.Queries.GetClinicMedicines;

public class GetClinicMedicinesQuery : IRequest<Result<List<ClinicMedicineDto>>>
{
    public string? SearchTerm { get; set; }
    public int? ClinicId { get; set; }
    public bool? IsActive { get; set; } // null = all, true = active only, false = inactive only
}

public class ClinicMedicineDto
{
    public int Id { get; set; }
    public int ClinicId { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public int? GlobalMedicineId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string GenericName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Potency { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

