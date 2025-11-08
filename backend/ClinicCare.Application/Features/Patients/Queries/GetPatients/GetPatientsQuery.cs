using ClinicCare.Application.Common.Models;
using MediatR;

namespace ClinicCare.Application.Features.Patients.Queries.GetPatients;

public class GetPatientsQuery : IRequest<Result<PaginatedResult<PatientDto>>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public string? Gender { get; set; }
    public string? BloodGroup { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public string? SortOrder { get; set; } = "desc";
}

