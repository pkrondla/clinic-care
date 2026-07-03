namespace HomoeoDesk.Global.Application.Common.Interfaces;

public interface ITenantService
{
    int? TenantId { get; }
    string? Subdomain { get; }
    Task<int> GetTenantIdAsync();
    Task<bool> IsValidTenantAsync(string subdomain);
}
