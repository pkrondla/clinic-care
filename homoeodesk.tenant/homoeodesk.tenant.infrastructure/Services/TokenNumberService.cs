using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Services;
using HomoeoDesk.Tenant.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Infrastructure.Services;

public class TokenNumberService : ITokenNumberService
{
    private readonly IApplicationDbContext _context;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public TokenNumberService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetNextTokenNumberAsync(int doctorId, int BranchId, DateOnly date, CancellationToken cancellationToken = default)
    {
        // Use semaphore to ensure thread-safe token generation
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            // Get the maximum token number for this doctor on this date
            // Only count non-cancelled appointments
            var maxToken = await _context.Appointments
                .Where(a => a.DoctorId == doctorId
                         && a.BranchId == BranchId
                         && a.AppointmentDate.Value == date
                         && a.Status != AppointmentStatus.Cancelled
                         && a.IsActive)
                .MaxAsync(a => (int?)a.TokenNumber, cancellationToken) ?? 0;

            // Return next token number
            return maxToken + 1;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}

