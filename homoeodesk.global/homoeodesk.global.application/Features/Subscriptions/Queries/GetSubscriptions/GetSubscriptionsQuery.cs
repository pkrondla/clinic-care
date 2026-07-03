using HomoeoDesk.Global.Application.Common.Interfaces.Global;
using HomoeoDesk.Global.Application.Common.Models;
using HomoeoDesk.Global.Application.Features.Subscriptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Global.Application.Features.Subscriptions.Queries.GetSubscriptions;

public class GetSubscriptionsQuery : IRequest<Result<List<SubscriptionDto>>>
{
    public int? OrganizationId { get; set; }
    public string? Plan { get; set; }
    public string? Status { get; set; }
    public string? Search { get; set; }
}

public class GetSubscriptionsHandler : IRequestHandler<GetSubscriptionsQuery, Result<List<SubscriptionDto>>>
{
    private readonly IGlobalDbContext _context;

    public GetSubscriptionsHandler(IGlobalDbContext context) => _context = context;

    public async Task<Result<List<SubscriptionDto>>> Handle(GetSubscriptionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.OrganizationSubscriptions
            .AsNoTracking()
            .Include(s => s.GlobalTenant)
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.PaymentTransactions)
            .AsQueryable();

        if (request.OrganizationId.HasValue)
            query = query.Where(s => s.TenantId == request.OrganizationId.Value);

        if (!string.IsNullOrWhiteSpace(request.Plan))
            query = query.Where(s => s.SubscriptionPlan.Name == request.Plan);

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var statusFilter = SubscriptionMapper.MapStatusForFilter(request.Status);
            query = query.Where(s => s.Status == statusFilter);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(s =>
                s.GlobalTenant.Name.Contains(term) ||
                s.SubscriptionPlan.Name.Contains(term));
        }

        var subscriptions = await query
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

        return Result<List<SubscriptionDto>>.Success(subscriptions.Select(SubscriptionMapper.ToDto).ToList());
    }
}
