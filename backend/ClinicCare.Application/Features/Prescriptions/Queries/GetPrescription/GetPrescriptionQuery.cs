using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Prescriptions.Commands.CreatePrescription;
using MediatR;

namespace ClinicCare.Application.Features.Prescriptions.Queries.GetPrescription;

public class GetPrescriptionQuery : IRequest<Result<PrescriptionDto>>
{
    public int Id { get; set; }
}

