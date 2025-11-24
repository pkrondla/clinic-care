using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.GlobalMedicines.Commands.CreateGlobalMedicine;
using MediatR;

namespace ClinicCare.Application.Features.GlobalMedicines.Queries.GetGlobalMedicines;

public class GetGlobalMedicinesQuery : IRequest<Result<List<GlobalMedicineDto>>>
{
    public string? SearchTerm { get; set; }
    public string? Type { get; set; }
    public string? Manufacturer { get; set; }
}

