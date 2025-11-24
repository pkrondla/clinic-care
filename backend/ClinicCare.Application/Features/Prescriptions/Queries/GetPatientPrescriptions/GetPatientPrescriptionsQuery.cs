using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Prescriptions.Commands.CreatePrescription;
using MediatR;

namespace ClinicCare.Application.Features.Prescriptions.Queries.GetPatientPrescriptions;

public class GetPatientPrescriptionsQuery : IRequest<Result<List<PrescriptionDto>>>
{
    public int PatientId { get; set; }
}

