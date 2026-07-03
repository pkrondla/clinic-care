using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Consultations.Commands.CreateConsultation;
using MediatR;

namespace HomoeoDesk.Tenant.Application.Features.Consultations.Queries.GetConsultations;

public class GetConsultationsQuery : IRequest<Result<List<ConsultationDto>>>
{
    public int? BranchId { get; set; }
    public int? DoctorId { get; set; }
    public int? PatientId { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}

