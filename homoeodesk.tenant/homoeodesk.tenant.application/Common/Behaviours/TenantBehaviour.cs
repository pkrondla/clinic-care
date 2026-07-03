using MediatR;

namespace HomoeoDesk.Tenant.Application.Common.Behaviours;

/// <summary>
/// Tenant and branch scoping is enforced via EF global query filters on <see cref="Infrastructure.Data.TenantDbContext"/>.
/// </summary>
public class TenantBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken) =>
        next();
}
