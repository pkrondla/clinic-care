using ClinicCare.Application.Common.Models;
using MediatR;

namespace ClinicCare.Application.Features.ClinicMedicines.Queries.SearchClinicMedicines;

public class SearchClinicMedicinesQuery : IRequest<Result<List<ClinicMedicineSearchDto>>>
{
    public string? SearchTerm { get; set; }
}

public class ClinicMedicineSearchDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string GenericName { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Potency { get; set; } = string.Empty;
}

