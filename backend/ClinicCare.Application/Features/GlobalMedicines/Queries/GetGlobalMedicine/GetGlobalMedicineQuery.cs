using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.GlobalMedicines.Commands.CreateGlobalMedicine;
using MediatR;

namespace ClinicCare.Application.Features.GlobalMedicines.Queries.GetGlobalMedicine;

public class GetGlobalMedicineQuery : IRequest<Result<GlobalMedicineDto>>
{
    public int Id { get; set; }
}

