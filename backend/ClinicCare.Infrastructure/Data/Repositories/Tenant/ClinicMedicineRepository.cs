using ClinicCare.Application.Common.Interfaces.Global;
using ClinicCare.Application.Common.Interfaces.Tenant;
using ClinicCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Infrastructure.Data.Repositories.Tenant;

public class ClinicMedicineRepository : IClinicMedicineRepository
{
    private readonly TenantDbContext _context;
    private readonly GlobalDbContext _globalContext;

    public ClinicMedicineRepository(TenantDbContext context, GlobalDbContext globalContext)
    {
        _context = context;
        _globalContext = globalContext;
    }

    public async Task<ClinicMedicine?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.ClinicMedicines
            .FirstOrDefaultAsync(m => m.Id == id && m.IsActive, cancellationToken);
    }

    public async Task<List<ClinicMedicine>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ClinicMedicines
            .Where(m => m.IsActive)
            .OrderBy(m => m.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ClinicMedicine>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var search = searchTerm.ToLower();
        return await _context.ClinicMedicines
            .Where(m => m.IsActive &&
                (m.Name.ToLower().Contains(search) ||
                 m.GenericName.ToLower().Contains(search) ||
                 m.Manufacturer.ToLower().Contains(search) ||
                 m.Type.ToLower().Contains(search)))
            .OrderBy(m => m.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<ClinicMedicine> AddAsync(ClinicMedicine medicine, CancellationToken cancellationToken = default)
    {
        _context.ClinicMedicines.Add(medicine);
        await _context.SaveChangesAsync(cancellationToken);
        return medicine;
    }

    public async Task<ClinicMedicine> AddFromGlobalAsync(int globalMedicineId, CancellationToken cancellationToken = default)
    {
        // Get the global medicine
        var globalMedicine = await _globalContext.GlobalMedicines
            .FirstOrDefaultAsync(m => m.Id == globalMedicineId && m.IsActive, cancellationToken);

        if (globalMedicine == null)
        {
            throw new InvalidOperationException($"Global medicine with ID {globalMedicineId} not found.");
        }

        // Check if already exists in clinic
        var exists = await ExistsAsync(globalMedicine.Name, globalMedicine.Potency, globalMedicine.Manufacturer, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException("Medicine already exists in clinic catalog.");
        }

        // Create clinic medicine from global medicine
        var clinicMedicine = new ClinicMedicine
        {
            GlobalMedicineId = globalMedicine.Id,
            Name = globalMedicine.Name,
            GenericName = globalMedicine.GenericName,
            Type = globalMedicine.Type,
            Potency = globalMedicine.Potency,
            Manufacturer = globalMedicine.Manufacturer,
            PurchasePrice = globalMedicine.Price,
            SellingPrice = globalMedicine.Price, // Default to same as purchase price
            Description = globalMedicine.Description,
            IsActive = true
        };

        return await AddAsync(clinicMedicine, cancellationToken);
    }

    public async Task UpdateAsync(ClinicMedicine medicine, CancellationToken cancellationToken = default)
    {
        _context.ClinicMedicines.Update(medicine);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var medicine = await _context.ClinicMedicines.FindAsync(new object[] { id }, cancellationToken);
        if (medicine != null)
        {
            medicine.IsActive = false; // Soft delete
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(string name, string potency, string manufacturer, CancellationToken cancellationToken = default)
    {
        return await _context.ClinicMedicines
            .AnyAsync(m => m.Name == name && m.Potency == potency && m.Manufacturer == manufacturer, cancellationToken);
    }
}

