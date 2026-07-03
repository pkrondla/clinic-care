using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Notifications.Queries.GetNotificationPreferences;

public class GetNotificationPreferencesHandler : IRequestHandler<GetNotificationPreferencesQuery, List<NotificationPreferenceDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetNotificationPreferencesHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<NotificationPreferenceDto>> Handle(GetNotificationPreferencesQuery request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUserService.OrganizationId;
        if (!organizationId.HasValue)
        {
            return new List<NotificationPreferenceDto>();
        }

        var preferences = await _context.NotificationPreferences
            .Where(p => p.OrganizationId == organizationId.Value && p.IsActive)
            .ToListAsync(cancellationToken);

        // Get all notification types and create DTOs
        var allNotificationTypes = Enum.GetValues<NotificationType>();
        var result = new List<NotificationPreferenceDto>();

        foreach (var notificationType in allNotificationTypes)
        {
            var preference = preferences.FirstOrDefault(p => p.NotificationType == notificationType);
            
            result.Add(new NotificationPreferenceDto
            {
                Id = preference?.Id ?? 0,
                NotificationType = notificationType,
                NotificationTypeName = GetNotificationTypeName(notificationType),
                EnableWhatsApp = preference?.EnableWhatsApp ?? true,
                EnableEmail = preference?.EnableEmail ?? true,
                EnableSMS = preference?.EnableSMS ?? false,
                Template = preference?.Template,
                IsActive = preference?.IsActive ?? true
            });
        }

        return result.OrderBy(r => r.NotificationType).ToList();
    }

    private string GetNotificationTypeName(NotificationType type)
    {
        return type switch
        {
            NotificationType.AppointmentCreated => "Appointment Created",
            NotificationType.AppointmentReminder => "Appointment Reminder",
            NotificationType.AppointmentCancelled => "Appointment Cancelled",
            NotificationType.TokenStatusUpdate => "Token Status Update",
            NotificationType.ConsultationCompleted => "Consultation Completed",
            NotificationType.PrescriptionCreated => "Prescription Created",
            NotificationType.PrescriptionReadyForCollection => "Prescription Ready for Collection",
            NotificationType.InvoiceCreated => "Invoice Created",
            NotificationType.PaymentReceived => "Payment Received",
            NotificationType.PaymentReminder => "Payment Reminder",
            NotificationType.CourierDispatched => "Courier Dispatched",
            NotificationType.CourierDelivered => "Courier Delivered",
            NotificationType.FollowUpReminder => "Follow-up Reminder",
            _ => type.ToString()
        };
    }
}

