using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Consultations.Commands.CreateConsultation;
using MediatR;

namespace ClinicCare.Application.Features.Consultations.Commands.UpdateConsultation;

public class UpdateConsultationCommand : IRequest<Result<ConsultationDto>>
{
    public int Id { get; set; }
    public string? ChiefComplaint { get; set; }
    public string? Symptoms { get; set; }
    public string? Examination { get; set; }
    public string? Diagnosis { get; set; }
    public string? TreatmentPlan { get; set; }
    public string? Notes { get; set; }
    public decimal? ConsultationFee { get; set; }
}

