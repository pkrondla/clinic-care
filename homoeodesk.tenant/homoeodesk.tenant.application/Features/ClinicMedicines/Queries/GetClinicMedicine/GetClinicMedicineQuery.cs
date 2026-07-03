using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.ClinicMedicines.Queries.GetClinicMedicines;
using MediatR;

namespace HomoeoDesk.Tenant.Application.Features.ClinicMedicines.Queries.GetClinicMedicine;

public class GetClinicMedicineQuery : IRequest<Result<ClinicMedicineDto>>
{
    public int Id { get; set; }
}

