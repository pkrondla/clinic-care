using HomoeoDesk.Global.Application.Common.Interfaces.Global;
using HomoeoDesk.Global.Application.Common.Models;
using HomoeoDesk.Global.Application.Features.Subscriptions;
using HomoeoDesk.Global.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Global.Application.Features.Subscriptions.Commands.UpdateSubscription;

public class UpdateSubscriptionCommand : IRequest<Result<SubscriptionDto>>
{
    public int Id { get; set; }
    public int? OrganizationId { get; set; }
    public string? Plan { get; set; }
    public string? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Price { get; set; }
    public int? MaxClinics { get; set; }
    public int? MaxUsers { get; set; }
    public string[]? Features { get; set; }
}

public class UpdateSubscriptionHandler : IRequestHandler<UpdateSubscriptionCommand, Result<SubscriptionDto>>
{
    private readonly IGlobalDbContext _context;

    public UpdateSubscriptionHandler(IGlobalDbContext context) => _context = context;

    public async Task<Result<SubscriptionDto>> Handle(UpdateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _context.OrganizationSubscriptions
            .Include(s => s.GlobalTenant)
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.PaymentTransactions)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (subscription == null)
            return Result<SubscriptionDto>.Failure("Subscription not found");

        if (request.StartDate.HasValue)
            subscription.StartDate = request.StartDate.Value;

        if (request.EndDate.HasValue)
            subscription.EndDate = request.EndDate.Value;

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            subscription.Status = SubscriptionMapper.MapStatusForFilter(request.Status);
            subscription.GlobalTenant.SubscriptionStatus = subscription.Status;
        }

        if (!string.IsNullOrWhiteSpace(request.Plan))
        {
            var plan = await _context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.Name == request.Plan, cancellationToken);

            if (plan == null)
                return Result<SubscriptionDto>.Failure($"Plan '{request.Plan}' not found");

            subscription.SubscriptionPlanId = plan.Id;
            subscription.SubscriptionPlan = plan;
        }

        var planEntity = subscription.SubscriptionPlan;

        if (request.Price.HasValue)
            planEntity.Price = request.Price.Value;

        if (request.MaxClinics.HasValue)
            planEntity.MaxClinics = request.MaxClinics.Value;

        if (request.MaxUsers.HasValue)
            planEntity.MaxDoctors = request.MaxUsers.Value;

        if (request.Features != null)
            planEntity.Features = string.Join(", ", request.Features);

        subscription.UpdatedAt = DateTime.UtcNow;
        planEntity.UpdatedAt = DateTime.UtcNow;
        subscription.GlobalTenant.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<SubscriptionDto>.Success(SubscriptionMapper.ToDto(subscription));
    }
}
