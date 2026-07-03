using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Doctors.Queries.GetDoctorAvailability;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace HomoeoDesk.Tenant.Application.Features.Doctors.Commands.CreateDoctorAvailability;

public class CreateDoctorAvailabilityCommand : IRequest<Result<DoctorAvailabilityDto>>
{
    [Required]
    public int DoctorId { get; set; }

    [Required]
    public int BranchId { get; set; }

    [Required]
    public DateOnly AvailableDate { get; set; }

    [Required]
    public TimeOnly StartTime { get; set; }

    [Required]
    public TimeOnly EndTime { get; set; }
}

