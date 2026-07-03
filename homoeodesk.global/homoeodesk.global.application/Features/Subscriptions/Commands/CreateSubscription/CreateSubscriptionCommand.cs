using HomoeoDesk.Global.Application.Common.Interfaces.Global;
using HomoeoDesk.Global.Application.Common.Models;
using HomoeoDesk.Global.Application.Features.Subscriptions;
using HomoeoDesk.Global.Domain.Entities;
using HomoeoDesk.Global.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Global.Application.Features.Subscriptions.Commands.CreateSubscription;

public class CreateSubscriptionCommand : IRequest<Result<SubscriptionDto>>
{
    public int OrganizationId { get; set; }
    public string Plan { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Price { get; set; }
    public int MaxClinics { get; set; }
    public int MaxUsers { get; set; }
    public string[] Features { get; set; } = Array.Empty<string>();
}

public class CreateSubscriptionHandler : IRequestHandler<CreateSubscriptionCommand, Result<SubscriptionDto>>
{
    private readonly IGlobalDbContext _context;

    public CreateSubscriptionHandler(IGlobalDbContext context) => _context = context;

    public async Task<Result<SubscriptionDto>> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _context.GlobalTenants
            .FirstOrDefaultAsync(t => t.Id == request.OrganizationId, cancellationToken);

        if (tenant == null)
            return Result<SubscriptionDto>.Failure("Organization not found");

        var plan = await _context.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Name == request.Plan, cancellationToken);

        if (plan == null)
        {
            plan = new SubscriptionPlan
            {
                Name = request.Plan,
                Description = $"{request.Plan} plan",
                Price = request.Price,
                BillingCycle = 1,
                MaxClinics = request.MaxClinics,
                MaxDoctors = request.MaxUsers,
                MaxPatients = 0,
                Features = string.Join(", ", request.Features),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.SubscriptionPlans.Add(plan);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var subscription = new OrganizationSubscription
        {
            TenantId = tenant.Id,
            SubscriptionPlanId = plan.Id,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = SubscriptionStatus.Active,
            AutoRenew = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.OrganizationSubscriptions.Add(subscription);
        tenant.SubscriptionStatus = SubscriptionStatus.Active;
        tenant.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        subscription.GlobalTenant = tenant;
        subscription.SubscriptionPlan = plan;

        return Result<SubscriptionDto>.Success(SubscriptionMapper.ToDto(subscription));
    }
}
