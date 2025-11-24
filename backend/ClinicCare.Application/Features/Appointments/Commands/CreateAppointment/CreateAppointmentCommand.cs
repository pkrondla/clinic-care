using MediatR;
using ClinicCare.Application.Common.Models;

namespace ClinicCare.Application.Features.Appointments.Commands.CreateAppointment
{
    public record CreateAppointmentCommand(
        int ClinicId,
        int DoctorId,
        int PatientId,
        DateOnly AppointmentDate,
        int? TokenNumber = null, // Optional - will be auto-generated if not provided
        int Type = 1, // Default to InPerson
        string Notes = ""
    ) : IRequest<Result<AppointmentDto>>;
}

