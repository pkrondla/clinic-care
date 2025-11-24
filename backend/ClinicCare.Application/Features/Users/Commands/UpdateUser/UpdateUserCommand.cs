using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Users.Queries.GetUsers;
using ClinicCare.Domain.Enums;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace ClinicCare.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommand : IRequest<Result<UserDto>>
{
    [Required]
    public int Id { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? Password { get; set; }

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; }

    public bool IsActive { get; set; } = true;

    public List<int> ClinicIds { get; set; } = new();

    // Doctor-specific fields
    public string? RegistrationNumber { get; set; }
    public string? Qualification { get; set; }
    public string? Specialization { get; set; }
    public int? ExperienceYears { get; set; }
    public decimal? ConsultationFeeInPerson { get; set; }
    public decimal? ConsultationFeeTele { get; set; }
    public decimal? FollowupFeeInPerson { get; set; }
    public decimal? FollowupFeeTele { get; set; }
}

