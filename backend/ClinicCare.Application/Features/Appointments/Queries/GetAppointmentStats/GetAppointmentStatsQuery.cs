using MediatR;
using ClinicCare.Application.Common.Models;

namespace ClinicCare.Application.Features.Appointments.Queries.GetAppointmentStats
{
    public record GetAppointmentStatsQuery(
        int? ClinicId = null,
        int? DoctorId = null
    ) : IRequest<Result<AppointmentStatsDto>>;
}

