using ClinicCare.Application.Common.Models;
using MediatR;

namespace ClinicCare.Application.Features.Appointments.Queries.GetAllQueues;

public class GetAllQueuesQuery : IRequest<Result<List<DoctorQueueDto>>>
{
    public int? ClinicId { get; set; }
    public DateOnly? Date { get; set; }
    public bool IncludePatientDetails { get; set; } = false; // For staff/doctor view
}

