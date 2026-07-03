namespace HomoeoDesk.Tenant.Application.Common.Interfaces;

/// <summary>
/// Marks a MediatR request that must run before tenant identity can be assumed to exist
/// (login, token refresh, logout) — mirrors the "/api/auth" skip-list in TenantMiddleware.
/// TenantBehaviour does not eagerly resolve the tenant for these; each handler is responsible
/// for its own tenant-resolution fallback if it needs one (see LoginCommandHandler).
/// </summary>
public interface ISkipTenantResolution
{
}
