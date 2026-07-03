using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Services;
using MediatR;

namespace HomoeoDesk.Tenant.Application.Features.Notifications.Queries.GetNotificationPreferences;

public class GetNotificationPreferencesHandler : IRequestHandler<GetNotificationPreferencesQuery, List<NotificationPreferenceDto>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationPreferencesReadService _notificationPreferencesReadService;

    public GetNotificationPreferencesHandler(
        ICurrentUserService currentUserService,
        INotificationPreferencesReadService notificationPreferencesReadService)
    {
        _currentUserService = currentUserService;
        _notificationPreferencesReadService = notificationPreferencesReadService;
    }

    public async Task<List<NotificationPreferenceDto>> Handle(GetNotificationPreferencesQuery request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUserService.OrganizationId;
        if (!organizationId.HasValue)
        {
            return new List<NotificationPreferenceDto>();
        }

        return await _notificationPreferencesReadService.GetPreferencesAsync(organizationId.Value, cancellationToken);
    }
}

