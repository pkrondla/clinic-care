using AutoMapper;
using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Branches.Commands.CreateBranch;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Branches.Queries.GetBranch;

public class GetBranchHandler : IRequestHandler<GetBranchQuery, Result<BranchDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetBranchHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<BranchDto>> Handle(GetBranchQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var clinic = await _context.Branches
                .FirstOrDefaultAsync(c => c.Id == request.Id && c.IsActive, cancellationToken);

            if (clinic == null)
            {
                return Result<BranchDto>.Failure(new[] { "Branch not found." });
            }

            var dto = _mapper.Map<BranchDto>(clinic);
            return Result<BranchDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<BranchDto>.Failure(new[] { $"Failed to retrieve clinic: {ex.Message}" });
        }
    }
}
