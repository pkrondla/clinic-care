using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Users.Queries.GetUsers;
using MediatR;

namespace ClinicCare.Application.Features.Users.Queries.GetUser;

public class GetUserQuery : IRequest<Result<UserDto>>
{
    public int Id { get; set; }
}

