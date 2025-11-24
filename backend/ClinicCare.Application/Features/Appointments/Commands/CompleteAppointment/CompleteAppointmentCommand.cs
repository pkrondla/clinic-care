using ClinicCare.Application.Common.Models;
using MediatR;

namespace ClinicCare.Application.Features.Appointments.Commands.CompleteAppointment;

public class CompleteAppointmentCommand : IRequest<Result<AppointmentDto>>
{
    public int Id { get; set; }
}

