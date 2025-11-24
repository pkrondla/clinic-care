using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Consultations.Commands.CreateConsultation;
using MediatR;

namespace ClinicCare.Application.Features.Consultations.Queries.GetConsultation;

public class GetConsultationQuery : IRequest<Result<ConsultationDto>>
{
    public int Id { get; set; }
}

