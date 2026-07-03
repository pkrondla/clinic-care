using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Consultations.Commands.CreateConsultation;
using MediatR;

namespace HomoeoDesk.Tenant.Application.Features.Consultations.Commands.UpdateConsultation;

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

