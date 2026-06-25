using ClinicCare.Domain.Enums;
using MediatR;

namespace ClinicCare.Application.Features.Notifications.Commands.UpdateNotificationPreferences;

public class UpdateNotificationPreferencesCommand : IRequest<List<NotificationPreferenceDto>>
{
    public List<NotificationPreferenceUpdateDto> Preferences { get; set; } = new();
}

public class NotificationPreferenceUpdateDto
{
    public NotificationType NotificationType { get; set; }
    public bool EnableWhatsApp { get; set; }
    public bool EnableEmail { get; set; }
    public bool EnableSMS { get; set; }
    public string? Template { get; set; }
    public bool IsActive { get; set; }
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

