using MediatR;
using ClinicCare.Application.Common.Models;

namespace ClinicCare.Application.Features.Appointments.Commands.CreateAppointment
{
    public record CreateAppointmentCommand(
        int ClinicId,
        int DoctorId,
        int PatientId,
        DateOnly AppointmentDate,
        int TokenNumber,
        int Type,
        string Notes = ""
    ) : IRequest<Result<AppointmentDto>>;
}

