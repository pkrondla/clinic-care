using ClinicCare.Application.Common.Interfaces.Tenant;
using ClinicCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Infrastructure.Data.Repositories.Tenant;

public class ClinicRepository : IClinicRepository
{
    private readonly TenantDbContext _context;

    public ClinicRepository(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<Clinic?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Clinics
            .FirstOrDefaultAsync(c => c.Id == id && c.IsActive, cancellationToken);
    }

    public async Task<List<Clinic>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Clinics
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ClinicRepository.GetAllAsync Error: {ex}");
            throw;
        }
    }

    public async Task<List<Clinic>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserClinicAccess
            .Where(uca => uca.UserId == userId && uca.CanAccess)
            .Include(uca => uca.Clinic)
            .Select(uca => uca.Clinic)
            .Where(c => c.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<Clinic> AddAsync(Clinic clinic, CancellationToken cancellationToken = default)
    {
        _context.Clinics.Add(clinic);
        await _context.SaveChangesAsync(cancellationToken);
        return clinic;
    }

    public async Task UpdateAsync(Clinic clinic, CancellationToken cancellationToken = default)
    {
        _context.Clinics.Update(clinic);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var clinic = await _context.Clinics.FindAsync(new object[] { id }, cancellationToken);
        if (clinic != null)
        {
            clinic.IsActive = false; // Soft delete
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Clinics.Where(c => c.Code == code);
        
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }
        
        return await query.AnyAsync(cancellationToken);
    }
}

