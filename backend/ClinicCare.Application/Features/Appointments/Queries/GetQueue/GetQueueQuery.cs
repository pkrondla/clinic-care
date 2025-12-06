using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Appointments.Queries.GetAllQueues;
using MediatR;

namespace ClinicCare.Application.Features.Appointments.Queries.GetQueue;

public class GetQueueQuery : IRequest<Result<DoctorQueueDto>>
{
    public int DoctorId { get; set; }
    public int? ClinicId { get; set; }
    public DateOnly? Date { get; set; }
    public bool IncludePatientDetails { get; set; } = true; // Default true for doctor/staff view
}

