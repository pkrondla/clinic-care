using HomoeoDesk.Global.Application.Common.Models;
using MediatR;

namespace HomoeoDesk.Global.Application.Features.GlobalMedicines.Commands.DeleteGlobalMedicine;

public class DeleteGlobalMedicineCommand : IRequest<Result<bool>>
{
    public int Id { get; set; }
}
