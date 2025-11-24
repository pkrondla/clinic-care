using ClinicCare.Application.Common.Interfaces.Global;
using ClinicCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Infrastructure.Data.Repositories.Global;

public class OrganizationRepository : IOrganizationRepository
{
    private readonly GlobalDbContext _context;

    public OrganizationRepository(GlobalDbContext context)
    {
        _context = context;
    }

    public async Task<Organization?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Organizations
            .Include(o => o.Subscriptions)
            .FirstOrDefaultAsync(o => o.Id == id && o.IsActive, cancellationToken);
    }

    public async Task<Organization?> GetBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default)
    {
        return await _context.Organizations
            .Include(o => o.Subscriptions)
            .FirstOrDefaultAsync(o => o.Subdomain == subdomain && o.IsActive, cancellationToken);
    }

    public async Task<List<Organization>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Organizations
            .Include(o => o.Subscriptions)
            .Where(o => o.IsActive)
            .OrderBy(o => o.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Organization> AddAsync(Organization organization, CancellationToken cancellationToken = default)
    {
        _context.Organizations.Add(organization);
        await _context.SaveChangesAsync(cancellationToken);
        return organization;
    }

    public async Task UpdateAsync(Organization organization, CancellationToken cancellationToken = default)
    {
        _context.Organizations.Update(organization);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var organization = await _context.Organizations.FindAsync(new object[] { id }, cancellationToken);
        if (organization != null)
        {
            organization.IsActive = false; // Soft delete
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> SubdomainExistsAsync(string subdomain, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Organizations.Where(o => o.Subdomain == subdomain);
        
        if (excludeId.HasValue)
        {
            query = query.Where(o => o.Id != excludeId.Value);
        }
        
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<string> GenerateSubdomainAsync(string organizationName, CancellationToken cancellationToken = default)
    {
        // Generate subdomain from organization name
        var baseSubdomain = organizationName
            .ToLower()
            .Replace(" ", "")
            .Replace("&", "and")
            .Where(c => char.IsLetterOrDigit(c))
            .Take(50)
            .Aggregate("", (current, c) => current + c);

        // Check if subdomain exists
        var subdomain = baseSubdomain;
        var counter = 1;

        while (await SubdomainExistsAsync(subdomain, null, cancellationToken))
        {
            subdomain = $"{baseSubdomain}{counter}";
            counter++;
        }

        return subdomain;
    }
}

