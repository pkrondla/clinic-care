using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Prescriptions.Commands.CreatePrescription;
using MediatR;

namespace HomoeoDesk.Tenant.Application.Features.Prescriptions.Queries.GetPatientPrescriptions;

public class GetPatientPrescriptionsQuery : IRequest<Result<List<PrescriptionDto>>>
{
    public int PatientId { get; set; }
}

