using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.ClinicMedicines.Queries.GetClinicMedicines;
using MediatR;

namespace ClinicCare.Application.Features.ClinicMedicines.Queries.GetClinicMedicine;

public class GetClinicMedicineQuery : IRequest<Result<ClinicMedicineDto>>
{
    public int Id { get; set; }
}

