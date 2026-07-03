namespace HomoeoDesk.Tenant.Application.Common.Services;

public interface IQueueNotificationService
{
    /// <summary>
    /// Broadcast queue update to all connected clients for a clinic
    /// </summary>
    Task BroadcastQueueUpdateAsync(int organizationId, int branchId, int? doctorId = null, CancellationToken cancellationToken = default);
}

