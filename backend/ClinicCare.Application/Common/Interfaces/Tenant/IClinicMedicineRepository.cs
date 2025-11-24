using ClinicCare.Domain.Entities;

namespace ClinicCare.Application.Common.Interfaces.Tenant;

/// <summary>
/// Repository for Clinic-specific Medicine catalog (Tenant Database)
/// </summary>
public interface IClinicMedicineRepository
{
    Task<ClinicMedicine?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<ClinicMedicine>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<ClinicMedicine>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<ClinicMedicine> AddAsync(ClinicMedicine medicine, CancellationToken cancellationToken = default);
    Task<ClinicMedicine> AddFromGlobalAsync(int globalMedicineId, CancellationToken cancellationToken = default);
    Task UpdateAsync(ClinicMedicine medicine, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string name, string potency, string manufacturer, CancellationToken cancellationToken = default);
}

