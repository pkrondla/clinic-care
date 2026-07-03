namespace HomoeoDesk.Tenant.Application.Common.Interfaces;

public interface ITenantService
{
    int TenantId { get; }
    int? OrganizationId { get; }
    string? Subdomain { get; }
    Task<int> GetOrganizationIdAsync();
    Task<int> GetTenantIdAsync();
}
