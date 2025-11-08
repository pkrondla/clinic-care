using ClinicCare.Application.Common.Models;
using MediatR;

namespace ClinicCare.Application.Features.Patients.Queries.GetPatient;

public class GetPatientQuery : IRequest<Result<PatientDetailDto>>
{
    public int Id { get; set; }
}

