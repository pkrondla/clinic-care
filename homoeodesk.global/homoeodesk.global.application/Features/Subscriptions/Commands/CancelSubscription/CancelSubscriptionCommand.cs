using HomoeoDesk.Global.Application.Common.Interfaces.Global;
using HomoeoDesk.Global.Application.Common.Models;
using HomoeoDesk.Global.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Global.Application.Features.Subscriptions.Commands.CancelSubscription;

public class CancelSubscriptionCommand : IRequest<Result>
{
    public int Id { get; set; }
    public string? Reason { get; set; }
}

public class CancelSubscriptionHandler : IRequestHandler<CancelSubscriptionCommand, Result>
{
    private readonly IGlobalDbContext _context;

    public CancelSubscriptionHandler(IGlobalDbContext context) => _context = context;

    public async Task<Result> Handle(CancelSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _context.OrganizationSubscriptions
            .Include(s => s.GlobalTenant)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (subscription == null)
            return Result.Failure("Subscription not found");

        subscription.Status = SubscriptionStatus.Cancelled;
        subscription.AutoRenew = false;
        subscription.UpdatedAt = DateTime.UtcNow;
        subscription.GlobalTenant.SubscriptionStatus = SubscriptionStatus.Cancelled;
        subscription.GlobalTenant.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
