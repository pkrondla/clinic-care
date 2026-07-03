using HomoeoDesk.Tenant.Application.Common.Interfaces;
using MediatR;

namespace HomoeoDesk.Tenant.Application.Common.Behaviours;

/// <summary>
/// Forces tenant resolution to happen at the very start of the pipeline, for every request,
/// instead of implicitly and lazily wherever a query filter first touches ITenantService.TenantId
/// deep inside a handler. ITenantService throws UnauthorizedAccessException if resolution fails
/// (ExceptionMiddleware maps that to a 401). Row-level tenant/branch scoping itself is still
/// enforced via EF global query filters on <see cref="Infrastructure.Data.TenantDbContext"/>.
/// </summary>
public class TenantBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ITenantService _tenantService;

    public TenantBehaviour(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not ISkipTenantResolution)
        {
            await _tenantService.GetTenantIdAsync();
        }

        return await next();
    }
}
