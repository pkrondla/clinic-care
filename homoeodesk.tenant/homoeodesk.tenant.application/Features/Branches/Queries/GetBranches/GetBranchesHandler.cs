using AutoMapper;
using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Branches.Commands.CreateBranch;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomoeoDesk.Tenant.Application.Features.Branches.Queries.GetBranches;

public class GetBranchesHandler : IRequestHandler<GetBranchesQuery, Result<List<BranchDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetBranchesHandler> _logger;

    public GetBranchesHandler(IApplicationDbContext context, IMapper mapper, ILogger<GetBranchesHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<BranchDto>>> Handle(GetBranchesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var Branches = await _context.Branches
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);

            var dtos = _mapper.Map<List<BranchDto>>(Branches);

            return Result<List<BranchDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Branches: {Message}", ex.Message);
            return Result<List<BranchDto>>.Failure(new[] { $"Failed to retrieve Branches: {ex.Message}" });
        }
    }
}
