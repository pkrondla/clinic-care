using MediatR;
using ClinicCare.Application.Common.Models;

namespace ClinicCare.Application.Features.Appointments.Queries.GetAppointment
{
    public record GetAppointmentQuery(int Id) : IRequest<Result<AppointmentDto>>;
}

