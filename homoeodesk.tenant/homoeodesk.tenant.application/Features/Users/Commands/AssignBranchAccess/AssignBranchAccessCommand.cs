using HomoeoDesk.Tenant.Application.Common.Models;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace HomoeoDesk.Tenant.Application.Features.Users.Commands.AssignBranchAccess;

public class AssignBranchAccessCommand : IRequest<Result<bool>>
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public List<int> BranchIds { get; set; } = new();
}

