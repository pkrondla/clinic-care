using ClinicCare.Application.Common.Models;
using MediatR;

namespace ClinicCare.Application.Features.Doctors.Queries.GetDoctorAvailability;

public class GetDoctorAvailabilityQuery : IRequest<Result<List<DoctorAvailabilityDto>>>
{
    public int? DoctorId { get; set; }
    public int? ClinicId { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}

public class DoctorAvailabilityDto
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public int ClinicId { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public DateOnly AvailableDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

