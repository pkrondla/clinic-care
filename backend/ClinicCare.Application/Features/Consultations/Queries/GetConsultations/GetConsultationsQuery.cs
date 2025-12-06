using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Consultations.Commands.CreateConsultation;
using MediatR;

namespace ClinicCare.Application.Features.Consultations.Queries.GetConsultations;

public class GetConsultationsQuery : IRequest<Result<List<ConsultationDto>>>
{
    public int? ClinicId { get; set; }
    public int? DoctorId { get; set; }
    public int? PatientId { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}

