using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Users.Queries.GetUsers;
using MediatR;

namespace HomoeoDesk.Tenant.Application.Features.Users.Queries.GetUser;

public class GetUserQuery : IRequest<Result<UserDto>>
{
    public int Id { get; set; }
}

