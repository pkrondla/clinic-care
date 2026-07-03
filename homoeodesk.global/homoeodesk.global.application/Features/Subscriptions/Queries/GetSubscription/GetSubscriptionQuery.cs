using HomoeoDesk.Global.Application.Common.Interfaces.Global;
using HomoeoDesk.Global.Application.Common.Models;
using HomoeoDesk.Global.Application.Features.Subscriptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Global.Application.Features.Subscriptions.Queries.GetSubscription;

public class GetSubscriptionQuery : IRequest<Result<SubscriptionDto>>
{
    public int? Id { get; set; }
    public int? OrganizationId { get; set; }
}

public class GetSubscriptionHandler : IRequestHandler<GetSubscriptionQuery, Result<SubscriptionDto>>
{
    private readonly IGlobalDbContext _context;

    public GetSubscriptionHandler(IGlobalDbContext context) => _context = context;

    public async Task<Result<SubscriptionDto>> Handle(GetSubscriptionQuery request, CancellationToken cancellationToken)
    {
        if (!request.Id.HasValue && !request.OrganizationId.HasValue)
            return Result<SubscriptionDto>.Failure("Subscription id or organization id is required");

        var query = _context.OrganizationSubscriptions
            .AsNoTracking()
            .Include(s => s.GlobalTenant)
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.PaymentTransactions)
            .AsQueryable();

        if (request.Id.HasValue)
            query = query.Where(s => s.Id == request.Id.Value);
        else
            query = query.Where(s => s.TenantId == request.OrganizationId!.Value);

        var subscription = await query
            .OrderByDescending(s => s.StartDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (subscription == null)
            return Result<SubscriptionDto>.Failure("Subscription not found");

        return Result<SubscriptionDto>.Success(SubscriptionMapper.ToDto(subscription));
    }
}
