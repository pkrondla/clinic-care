using HomoeoDesk.Global.Application.Common.Models;
using HomoeoDesk.Global.Application.Features.GlobalMedicines.Commands.CreateGlobalMedicine;
using MediatR;

namespace HomoeoDesk.Global.Application.Features.GlobalMedicines.Queries.GetGlobalMedicine;

public class GetGlobalMedicineQuery : IRequest<Result<GlobalMedicineDto>>
{
    public int Id { get; set; }
}
