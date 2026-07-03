using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Doctors.Queries.GetDoctorAvailability;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace HomoeoDesk.Tenant.Application.Features.Doctors.Commands.UpdateDoctorAvailability;

public class UpdateDoctorAvailabilityCommand : IRequest<Result<DoctorAvailabilityDto>>
{
    [Required]
    public int Id { get; set; }

    [Required]
    public DateOnly AvailableDate { get; set; }

    [Required]
    public TimeOnly StartTime { get; set; }

    [Required]
    public TimeOnly EndTime { get; set; }

    public bool IsActive { get; set; } = true;
}

