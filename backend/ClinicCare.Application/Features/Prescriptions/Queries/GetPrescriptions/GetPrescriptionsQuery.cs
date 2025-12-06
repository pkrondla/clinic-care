using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Prescriptions.Commands.CreatePrescription;
using MediatR;

namespace ClinicCare.Application.Features.Prescriptions.Queries.GetPrescriptions;

public class GetPrescriptionsQuery : IRequest<Result<List<PrescriptionDto>>>
{
    public int? ClinicId { get; set; }
    public int? DoctorId { get; set; }
    public int? PatientId { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}

