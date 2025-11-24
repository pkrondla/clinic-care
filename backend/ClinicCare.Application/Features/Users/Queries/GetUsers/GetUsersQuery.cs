using ClinicCare.Application.Common.Models;
using ClinicCare.Domain.Enums;
using MediatR;

namespace ClinicCare.Application.Features.Users.Queries.GetUsers;

public class GetUsersQuery : IRequest<Result<List<UserDto>>>
{
    public string? SearchTerm { get; set; }
    public UserRole? Role { get; set; }
    public int? ClinicId { get; set; }
    public bool? IsActive { get; set; }
}

