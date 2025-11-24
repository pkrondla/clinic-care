using ClinicCare.Domain.Entities;

namespace ClinicCare.Application.Common.Interfaces.Tenant;

/// <summary>
/// Repository for Clinic management (Tenant Database)
/// </summary>
public interface IClinicRepository
{
    Task<Clinic?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Clinic>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Clinic>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<Clinic> AddAsync(Clinic clinic, CancellationToken cancellationToken = default);
    Task UpdateAsync(Clinic clinic, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken cancellationToken = default);
}

