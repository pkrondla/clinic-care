using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Branches.Commands.CreateBranch;
using MediatR;

namespace HomoeoDesk.Tenant.Application.Features.Branches.Queries.GetBranches;

public class GetBranchesQuery : IRequest<Result<List<BranchDto>>>
{
    // All Branches for current organization
}

