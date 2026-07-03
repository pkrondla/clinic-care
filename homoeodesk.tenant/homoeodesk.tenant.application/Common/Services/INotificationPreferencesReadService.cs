using HomoeoDesk.Tenant.Application.Features.Notifications.Queries.GetNotificationPreferences;

namespace HomoeoDesk.Tenant.Application.Common.Services;

/// <summary>
/// Builds the full notification preference list (one entry per NotificationType, defaulted
/// where no row exists yet). Shared by GetNotificationPreferencesHandler and
/// UpdateNotificationPreferencesHandler so the latter doesn't have to hand-construct the former.
/// </summary>
public interface INotificationPreferencesReadService
{
    Task<List<NotificationPreferenceDto>> GetPreferencesAsync(int organizationId, CancellationToken cancellationToken);
}
