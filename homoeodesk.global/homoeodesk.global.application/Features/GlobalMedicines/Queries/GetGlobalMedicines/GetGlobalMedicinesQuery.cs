using HomoeoDesk.Global.Application.Common.Models;
using HomoeoDesk.Global.Application.Features.GlobalMedicines.Commands.CreateGlobalMedicine;
using MediatR;

namespace HomoeoDesk.Global.Application.Features.GlobalMedicines.Queries.GetGlobalMedicines;

public class GetGlobalMedicinesQuery : IRequest<Result<List<GlobalMedicineDto>>>
{
    public string? SearchTerm { get; set; }
    public string? Type { get; set; }
    public string? Manufacturer { get; set; }
}
