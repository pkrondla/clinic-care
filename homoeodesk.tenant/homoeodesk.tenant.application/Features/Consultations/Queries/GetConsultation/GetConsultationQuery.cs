using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Consultations.Commands.CreateConsultation;
using MediatR;

namespace HomoeoDesk.Tenant.Application.Features.Consultations.Queries.GetConsultation;

public class GetConsultationQuery : IRequest<Result<ConsultationDto>>
{
    public int Id { get; set; }
}

