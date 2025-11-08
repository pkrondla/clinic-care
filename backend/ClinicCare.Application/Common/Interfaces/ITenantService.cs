namespace ClinicCare.Application.Common.Interfaces;

public interface ITenantService
{
    int? OrganizationId { get; }
    string? Subdomain { get; }
    Task<int> GetOrganizationIdAsync();
    Task<bool> IsValidTenantAsync(string subdomain);
}
