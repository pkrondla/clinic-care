using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Domain.Enums;
using MediatR;

namespace HomoeoDesk.Tenant.Application.Features.Users.Queries.GetUsers;

public class GetUsersQuery : IRequest<Result<List<UserDto>>>
{
    public string? SearchTerm { get; set; }
    public UserRole? Role { get; set; }
    public int? BranchId { get; set; }
    public bool? IsActive { get; set; }
}

