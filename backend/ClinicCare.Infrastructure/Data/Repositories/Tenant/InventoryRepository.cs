using ClinicCare.Application.Common.Interfaces.Tenant;
using ClinicCare.Domain.Entities;
using ClinicCare.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Infrastructure.Data.Repositories.Tenant;

public class InventoryRepository : IInventoryRepository
{
    private readonly TenantDbContext _context;

    public InventoryRepository(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<Inventory?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Inventories
            .Include(i => i.Medicine)
            .Include(i => i.Clinic)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<List<Inventory>> GetByClinicIdAsync(int clinicId, CancellationToken cancellationToken = default)
    {
        return await _context.Inventories
            .Include(i => i.Medicine)
            .Where(i => i.ClinicId == clinicId)
            .OrderBy(i => i.Medicine.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Inventory>> GetLowStockAsync(int clinicId, CancellationToken cancellationToken = default)
    {
        return await _context.Inventories
            .Include(i => i.Medicine)
            .Where(i => i.ClinicId == clinicId && i.CurrentStock <= i.MinimumStock)
            .OrderBy(i => i.CurrentStock)
            .ToListAsync(cancellationToken);
    }

    public async Task<Inventory?> GetByClinicAndMedicineAsync(int clinicId, int medicineId, CancellationToken cancellationToken = default)
    {
        return await _context.Inventories
            .Include(i => i.Medicine)
            .FirstOrDefaultAsync(i => i.ClinicId == clinicId && i.MedicineId == medicineId, cancellationToken);
    }

    public async Task<Inventory> AddAsync(Inventory inventory, CancellationToken cancellationToken = default)
    {
        _context.Inventories.Add(inventory);
        await _context.SaveChangesAsync(cancellationToken);
        return inventory;
    }

    public async Task UpdateAsync(Inventory inventory, CancellationToken cancellationToken = default)
    {
        inventory.LastUpdated = DateTime.UtcNow;
        _context.Inventories.Update(inventory);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var inventory = await _context.Inventories.FindAsync(new object[] { id }, cancellationToken);
        if (inventory != null)
        {
            _context.Inventories.Remove(inventory);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> UpdateStockAsync(int inventoryId, int quantity, string transactionType, CancellationToken cancellationToken = default)
    {
        var inventory = await _context.Inventories.FindAsync(new object[] { inventoryId }, cancellationToken);
        if (inventory == null)
        {
            return false;
        }

        // Update stock based on transaction type
        switch (transactionType.ToLower())
        {
            case "purchase":
            case "restock":
            case "return":
                inventory.CurrentStock += quantity;
                break;
            
            case "sale":
            case "dispensing":
            case "wastage":
                inventory.CurrentStock -= quantity;
                break;
            
            case "adjustment":
                inventory.CurrentStock = quantity; // Set to exact quantity
                break;
            
            default:
                return false;
        }

        // Ensure stock doesn't go negative
        if (inventory.CurrentStock < 0)
        {
            return false;
        }

        inventory.LastUpdated = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        
        return true;
    }
}

