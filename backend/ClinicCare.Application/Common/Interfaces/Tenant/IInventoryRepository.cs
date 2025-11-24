using ClinicCare.Domain.Entities;

namespace ClinicCare.Application.Common.Interfaces.Tenant;

/// <summary>
/// Repository for Inventory management (Tenant Database)
/// </summary>
public interface IInventoryRepository
{
    Task<Inventory?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Inventory>> GetByClinicIdAsync(int clinicId, CancellationToken cancellationToken = default);
    Task<List<Inventory>> GetLowStockAsync(int clinicId, CancellationToken cancellationToken = default);
    Task<Inventory?> GetByClinicAndMedicineAsync(int clinicId, int medicineId, CancellationToken cancellationToken = default);
    Task<Inventory> AddAsync(Inventory inventory, CancellationToken cancellationToken = default);
    Task UpdateAsync(Inventory inventory, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> UpdateStockAsync(int inventoryId, int quantity, string transactionType, CancellationToken cancellationToken = default);
}

