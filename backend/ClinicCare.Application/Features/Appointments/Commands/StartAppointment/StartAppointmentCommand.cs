using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Appointments.Queries.GetAppointments;
using MediatR;

namespace ClinicCare.Application.Features.Appointments.Commands.StartAppointment;

public class StartAppointmentCommand : IRequest<Result<AppointmentDto>>
{
    public int Id { get; set; }
}

