using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Appointments.Queries.GetAllQueues;
using MediatR;

namespace HomoeoDesk.Tenant.Application.Features.Appointments.Queries.GetQueue;

public class GetQueueQuery : IRequest<Result<DoctorQueueDto>>
{
    public int DoctorId { get; set; }
    public int? BranchId { get; set; }
    public DateOnly? Date { get; set; }
    public bool IncludePatientDetails { get; set; } = true; // Default true for doctor/staff view
}

