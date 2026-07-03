using MediatR;
using HomoeoDesk.Tenant.Application.Common.Models;

namespace HomoeoDesk.Tenant.Application.Features.Appointments.Queries.GetAppointments
{
    public record GetAppointmentsQuery(
        int? BranchId = null,
        int? DoctorId = null,
        DateOnly? Date = null,
        int? Status = null
    ) : IRequest<Result<List<AppointmentDto>>>;
}

