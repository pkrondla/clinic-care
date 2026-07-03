using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Consultations.Commands.CreateConsultation;
using MediatR;

namespace HomoeoDesk.Tenant.Application.Features.Consultations.Queries.GetPatientConsultations;

public class GetPatientConsultationsQuery : IRequest<Result<List<ConsultationDto>>>
{
    public int PatientId { get; set; }
}

