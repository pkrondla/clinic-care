using ClinicCare.Domain.Entities;

namespace ClinicCare.Application.Common.Interfaces.Global;

/// <summary>
/// Repository for Global Medicine Database (Super Admin managed)
/// </summary>
public interface IGlobalMedicineRepository
{
    Task<GlobalMedicine?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<GlobalMedicine>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<GlobalMedicine>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<List<GlobalMedicine>> GetByTypeAsync(string type, CancellationToken cancellationToken = default);
    Task<List<GlobalMedicine>> GetByManufacturerAsync(string manufacturer, CancellationToken cancellationToken = default);
    Task<GlobalMedicine> AddAsync(GlobalMedicine medicine, CancellationToken cancellationToken = default);
    Task UpdateAsync(GlobalMedicine medicine, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string name, string potency, string manufacturer, CancellationToken cancellationToken = default);
}

