using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Branches.Commands.CreateBranch;
using MediatR;

namespace HomoeoDesk.Tenant.Application.Features.Branches.Queries.GetBranch;

public class GetBranchQuery : IRequest<Result<BranchDto>>
{
    public int Id { get; set; }
}

