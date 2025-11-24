using ClinicCare.Application.Common.Models;
using MediatR;

namespace ClinicCare.Application.Features.Appointments.Commands.StartAppointment;

public class StartAppointmentCommand : IRequest<Result<AppointmentDto>>
{
    public int Id { get; set; }
}

