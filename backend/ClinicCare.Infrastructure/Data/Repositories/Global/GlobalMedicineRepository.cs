using ClinicCare.Application.Common.Interfaces.Global;
using ClinicCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Infrastructure.Data.Repositories.Global;

public class GlobalMedicineRepository : IGlobalMedicineRepository
{
    private readonly GlobalDbContext _context;

    public GlobalMedicineRepository(GlobalDbContext context)
    {
        _context = context;
    }

    public async Task<GlobalMedicine?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.GlobalMedicines
            .FirstOrDefaultAsync(m => m.Id == id && m.IsActive, cancellationToken);
    }

    public async Task<List<GlobalMedicine>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.GlobalMedicines
            .Where(m => m.IsActive)
            .OrderBy(m => m.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<GlobalMedicine>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var search = searchTerm.ToLower();
        return await _context.GlobalMedicines
            .Where(m => m.IsActive &&
                (m.Name.ToLower().Contains(search) ||
                 m.GenericName.ToLower().Contains(search) ||
                 m.Manufacturer.ToLower().Contains(search) ||
                 m.Type.ToLower().Contains(search)))
            .OrderBy(m => m.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<GlobalMedicine>> GetByTypeAsync(string type, CancellationToken cancellationToken = default)
    {
        return await _context.GlobalMedicines
            .Where(m => m.IsActive && m.Type == type)
            .OrderBy(m => m.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<GlobalMedicine>> GetByManufacturerAsync(string manufacturer, CancellationToken cancellationToken = default)
    {
        return await _context.GlobalMedicines
            .Where(m => m.IsActive && m.Manufacturer == manufacturer)
            .OrderBy(m => m.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<GlobalMedicine> AddAsync(GlobalMedicine medicine, CancellationToken cancellationToken = default)
    {
        _context.GlobalMedicines.Add(medicine);
        await _context.SaveChangesAsync(cancellationToken);
        return medicine;
    }

    public async Task UpdateAsync(GlobalMedicine medicine, CancellationToken cancellationToken = default)
    {
        _context.GlobalMedicines.Update(medicine);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var medicine = await _context.GlobalMedicines.FindAsync(new object[] { id }, cancellationToken);
        if (medicine != null)
        {
            medicine.IsActive = false; // Soft delete
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(string name, string potency, string manufacturer, CancellationToken cancellationToken = default)
    {
        return await _context.GlobalMedicines
            .AnyAsync(m => m.Name == name && m.Potency == potency && m.Manufacturer == manufacturer, cancellationToken);
    }
}

