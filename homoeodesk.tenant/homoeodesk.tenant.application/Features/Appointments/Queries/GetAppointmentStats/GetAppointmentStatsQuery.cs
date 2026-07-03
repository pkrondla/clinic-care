using MediatR;
using HomoeoDesk.Tenant.Application.Common.Models;

namespace HomoeoDesk.Tenant.Application.Features.Appointments.Queries.GetAppointmentStats
{
    public record GetAppointmentStatsQuery(
        int? BranchId = null,
        int? DoctorId = null
    ) : IRequest<Result<AppointmentStatsDto>>;
}

