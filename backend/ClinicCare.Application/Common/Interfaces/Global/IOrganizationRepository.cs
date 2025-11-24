using ClinicCare.Domain.Entities;

namespace ClinicCare.Application.Common.Interfaces.Global;

/// <summary>
/// Repository for Organization management (Global Database)
/// </summary>
public interface IOrganizationRepository
{
    Task<Organization?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Organization?> GetBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default);
    Task<List<Organization>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Organization> AddAsync(Organization organization, CancellationToken cancellationToken = default);
    Task UpdateAsync(Organization organization, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> SubdomainExistsAsync(string subdomain, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<string> GenerateSubdomainAsync(string organizationName, CancellationToken cancellationToken = default);
}

