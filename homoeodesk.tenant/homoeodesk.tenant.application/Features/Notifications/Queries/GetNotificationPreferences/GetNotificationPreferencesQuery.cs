using HomoeoDesk.Tenant.Domain.Enums;
using MediatR;

namespace HomoeoDesk.Tenant.Application.Features.Notifications.Queries.GetNotificationPreferences;

public class GetNotificationPreferencesQuery : IRequest<List<NotificationPreferenceDto>>
{
}

public class NotificationPreferenceDto
{
    public int Id { get; set; }
    public NotificationType NotificationType { get; set; }
    public string NotificationTypeName { get; set; } = string.Empty;
    public bool EnableWhatsApp { get; set; }
    public bool EnableEmail { get; set; }
    public bool EnableSMS { get; set; }
    public string? Template { get; set; }
    public bool IsActive { get; set; }
}

