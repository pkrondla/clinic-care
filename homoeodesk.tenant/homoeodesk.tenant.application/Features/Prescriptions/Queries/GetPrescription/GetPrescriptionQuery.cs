using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Prescriptions.Commands.CreatePrescription;
using MediatR;

namespace HomoeoDesk.Tenant.Application.Features.Prescriptions.Queries.GetPrescription;

public class GetPrescriptionQuery : IRequest<Result<PrescriptionDto>>
{
    public int Id { get; set; }
}

