using HomoeoDesk.Tenant.Application.Common.Models;
using MediatR;

namespace HomoeoDesk.Tenant.Application.Features.Patients.Queries.GetPatient;

public class GetPatientQuery : IRequest<Result<PatientDetailDto>>
{
    public int Id { get; set; }
}

