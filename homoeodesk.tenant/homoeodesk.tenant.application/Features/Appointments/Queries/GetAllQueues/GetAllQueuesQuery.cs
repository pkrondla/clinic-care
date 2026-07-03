using HomoeoDesk.Tenant.Application.Common.Models;
using MediatR;

namespace HomoeoDesk.Tenant.Application.Features.Appointments.Queries.GetAllQueues;

public class GetAllQueuesQuery : IRequest<Result<List<DoctorQueueDto>>>
{
    public int? BranchId { get; set; }
    public DateOnly? Date { get; set; }
    public bool IncludePatientDetails { get; set; } = false; // For staff/doctor view
}

