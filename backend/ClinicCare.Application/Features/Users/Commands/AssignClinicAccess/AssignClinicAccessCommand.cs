using ClinicCare.Application.Common.Models;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace ClinicCare.Application.Features.Users.Commands.AssignClinicAccess;

public class AssignClinicAccessCommand : IRequest<Result<bool>>
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public List<int> ClinicIds { get; set; } = new();
}

