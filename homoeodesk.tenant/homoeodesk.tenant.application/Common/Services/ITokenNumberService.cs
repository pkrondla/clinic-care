namespace HomoeoDesk.Tenant.Application.Common.Services;

public interface ITokenNumberService
{
    /// <summary>
    /// Gets the next sequential token number for a doctor on a specific date
    /// </summary>
    /// <param name="doctorId">Doctor ID</param>
    /// <param name="BranchId">Clinic ID</param>
    /// <param name="date">Date for the appointment</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Next token number</returns>
    Task<int> GetNextTokenNumberAsync(int doctorId, int BranchId, DateOnly date, CancellationToken cancellationToken = default);
}

