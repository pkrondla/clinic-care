using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Services;
using HomoeoDesk.Tenant.Application.Features.Notifications.Queries.GetNotificationPreferences;
using HomoeoDesk.Tenant.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Notifications.Commands.UpdateNotificationPreferences;

public class UpdateNotificationPreferencesHandler : IRequestHandler<UpdateNotificationPreferencesCommand, List<NotificationPreferenceDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationPreferencesReadService _notificationPreferencesReadService;

    public UpdateNotificationPreferencesHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        INotificationPreferencesReadService notificationPreferencesReadService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _notificationPreferencesReadService = notificationPreferencesReadService;
    }

    public async Task<List<NotificationPreferenceDto>> Handle(UpdateNotificationPreferencesCommand request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUserService.OrganizationId;
        if (!organizationId.HasValue)
        {
            throw new UnauthorizedAccessException("User not associated with any organization");
        }

        foreach (var prefUpdate in request.Preferences)
        {
            var existing = await _context.NotificationPreferences
                .FirstOrDefaultAsync(
                    p => p.OrganizationId == organizationId.Value 
                      && p.NotificationType == prefUpdate.NotificationType,
                    cancellationToken);

            if (existing != null)
            {
                // Update existing
                existing.EnableWhatsApp = prefUpdate.EnableWhatsApp;
                existing.EnableEmail = prefUpdate.EnableEmail;
                existing.EnableSMS = prefUpdate.EnableSMS;
                existing.Template = prefUpdate.Template;
                existing.IsActive = prefUpdate.IsActive;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Create new
                var newPreference = new NotificationPreferences
                {
                    OrganizationId = organizationId.Value,
                    NotificationType = prefUpdate.NotificationType,
                    EnableWhatsApp = prefUpdate.EnableWhatsApp,
                    EnableEmail = prefUpdate.EnableEmail,
                    EnableSMS = prefUpdate.EnableSMS,
                    Template = prefUpdate.Template,
                    IsActive = prefUpdate.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.NotificationPreferences.Add(newPreference);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Return updated preferences
        var updated = await _notificationPreferencesReadService.GetPreferencesAsync(organizationId.Value, cancellationToken);
        return updated.Select(p => new NotificationPreferenceDto
        {
            NotificationType = p.NotificationType,
            EnableWhatsApp = p.EnableWhatsApp,
            EnableEmail = p.EnableEmail,
            EnableSMS = p.EnableSMS,
            Template = p.Template,
            IsActive = p.IsActive
        }).ToList();
    }
}

