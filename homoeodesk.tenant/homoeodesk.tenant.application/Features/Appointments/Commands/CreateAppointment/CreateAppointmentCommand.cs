using MediatR;
using HomoeoDesk.Tenant.Application.Common.Models;

namespace HomoeoDesk.Tenant.Application.Features.Appointments.Commands.CreateAppointment
{
    public record CreateAppointmentCommand(
        int BranchId,
        int DoctorId,
        int PatientId,
        DateOnly AppointmentDate,
        int? TokenNumber = null, // Optional - will be auto-generated if not provided
        int Type = 1, // Default to InPerson
        string Notes = ""
    ) : IRequest<Result<AppointmentDto>>;
}

