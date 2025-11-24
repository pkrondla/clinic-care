using ClinicCare.Application.Common.Services;
using ClinicCare.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;

namespace ClinicCare.Infrastructure.Services;

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

    public async Task BroadcastQueueUpdateAsync(int organizationId, int clinicId, int? doctorId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Broadcast to clinic group (all queues for the clinic)
            var clinicGroup = $"Clinic_{organizationId}_{clinicId}";
            await _hubContext.Clients.Group(clinicGroup).SendAsync("QueueUpdated", new
            {
                organizationId,
                clinicId,
                doctorId,
                timestamp = DateTime.UtcNow
            }, cancellationToken);

            // Also broadcast to specific doctor queue group if specified
            if (doctorId.HasValue)
            {
                var doctorGroup = $"Queue_{organizationId}_{clinicId}_{doctorId.Value}";
                await _hubContext.Clients.Group(doctorGroup).SendAsync("QueueUpdated", new
                {
                    organizationId,
                    clinicId,
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

