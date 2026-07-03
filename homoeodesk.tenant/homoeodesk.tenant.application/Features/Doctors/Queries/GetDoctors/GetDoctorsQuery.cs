using HomoeoDesk.Tenant.Application.Common.Models;
using MediatR;

namespace HomoeoDesk.Tenant.Application.Features.Doctors.Queries.GetDoctors;

public class GetDoctorsQuery : IRequest<Result<List<DoctorDto>>>
{
    public int? BranchId { get; set; }
    public bool? IsActive { get; set; } = true;
}

