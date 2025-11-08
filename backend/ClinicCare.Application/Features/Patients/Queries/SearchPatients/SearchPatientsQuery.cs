using ClinicCare.Application.Common.Models;
using MediatR;

namespace ClinicCare.Application.Features.Patients.Queries.SearchPatients;

public class SearchPatientsQuery : IRequest<Result<List<PatientSearchDto>>>
{
    public string SearchTerm { get; set; } = string.Empty;
    public int Limit { get; set; } = 10;
}

