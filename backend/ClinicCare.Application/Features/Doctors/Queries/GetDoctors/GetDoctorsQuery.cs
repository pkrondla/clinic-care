using ClinicCare.Application.Common.Models;
using MediatR;

namespace ClinicCare.Application.Features.Doctors.Queries.GetDoctors;

public class GetDoctorsQuery : IRequest<Result<List<DoctorDto>>>
{
    public int? ClinicId { get; set; }
    public bool? IsActive { get; set; } = true;
}

