using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Prescriptions.Commands.CreatePrescription;
using MediatR;

namespace HomoeoDesk.Tenant.Application.Features.Prescriptions.Queries.GetPrescriptions;

public class GetPrescriptionsQuery : IRequest<Result<List<PrescriptionDto>>>
{
    public int? BranchId { get; set; }
    public int? DoctorId { get; set; }
    public int? PatientId { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}

