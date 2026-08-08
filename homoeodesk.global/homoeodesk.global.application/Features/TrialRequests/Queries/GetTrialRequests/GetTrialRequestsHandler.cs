using HomoeoDesk.Global.Application.Common.Interfaces.Global;
using HomoeoDesk.Global.Application.Common.Models;
using HomoeoDesk.Global.Application.Features.TrialRequests.Commands.CreateTrialRequest;
using HomoeoDesk.Global.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Global.Application.Features.TrialRequests.Queries.GetTrialRequests;

public class GetTrialRequestsHandler : IRequestHandler<GetTrialRequestsQuery, Result<List<TrialRequestDto>>>
{
    private readonly IGlobalDbContext _context;

    public GetTrialRequestsHandler(IGlobalDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<TrialRequestDto>>> Handle(GetTrialRequestsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.TrialRequests.AsNoTracking().Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(request.Status)
                && Enum.TryParse<TrialRequestStatus>(request.Status, true, out var status))
            {
                query = query.Where(x => x.Status == status);
            }

            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Take(200)
                .ToListAsync(cancellationToken);

            return Result<List<TrialRequestDto>>.Success(
                items.Select(CreateTrialRequestHandler.Map).ToList());
        }
        catch (Exception ex)
        {
            return Result<List<TrialRequestDto>>.Failure($"Failed to load trial requests: {ex.Message}");
        }
    }
}
