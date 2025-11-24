using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Consultations.Commands.CreateConsultation;
using MediatR;

namespace ClinicCare.Application.Features.Consultations.Queries.GetPatientConsultations;

public class GetPatientConsultationsQuery : IRequest<Result<List<ConsultationDto>>>
{
    public int PatientId { get; set; }
}

