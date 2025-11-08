using MediatR;
using ClinicCare.Application.Common.Models;

namespace ClinicCare.Application.Features.Appointments.Queries.GetAppointments
{
    public record GetAppointmentsQuery(
        int? ClinicId = null,
        int? DoctorId = null,
        DateOnly? Date = null,
        int? Status = null
    ) : IRequest<Result<List<AppointmentDto>>>;
}

