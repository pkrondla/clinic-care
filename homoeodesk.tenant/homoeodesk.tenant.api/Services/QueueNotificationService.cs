using HomoeoDesk.Tenant.Application.Common.Services;
using HomoeoDesk.Tenant.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;

namespace HomoeoDesk.Tenant.Api.Services;

public class QueueNotificationService : IQueueNotificationService
{
    private readonly IHubContext<QueueHub> _hubContext;
    private readonly INotificationService? _notificationService;
    private readonly IConfiguration? _configuration;

    public QueueNotificationService(
        IHubContext<QueueHub> hubContext,
        INotificationService? notificationService = null,
        IConfiguration? configuration = null)
    {
        _hubContext = hubContext;
        _notificationService = notificationService;
        _configuration = configuration;
    }

    public async Task BroadcastQueueUpdateAsync(int organizationId, int branchId, int? doctorId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var branchGroup = $"Branch_{organizationId}_{branchId}";
            await _hubContext.Clients.Group(branchGroup).SendAsync("QueueUpdated", new
            {
                organizationId,
                branchId,
                doctorId,
                timestamp = DateTime.UtcNow
            }, cancellationToken);

            if (doctorId.HasValue)
            {
                var doctorGroup = $"Queue_{organizationId}_{branchId}_{doctorId.Value}";
                await _hubContext.Clients.Group(doctorGroup).SendAsync("QueueUpdated", new
                {
                    organizationId,
                    branchId,
                    doctorId = doctorId.Value,
                    timestamp = DateTime.UtcNow
                }, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            // Log error but don't throw - queue updates should not break the main flow
            Console.WriteLine($"Error broadcasting queue update: {ex.Message}");
        }
    }
}

