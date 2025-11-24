using ClinicCare.Application.Common.Models;
using MediatR;

namespace ClinicCare.Application.Features.GlobalMedicines.Commands.DeleteGlobalMedicine;

public class DeleteGlobalMedicineCommand : IRequest<Result<bool>>
{
    public int Id { get; set; }
}

