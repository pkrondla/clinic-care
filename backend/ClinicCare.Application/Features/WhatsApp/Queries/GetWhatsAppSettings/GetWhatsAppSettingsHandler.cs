using ClinicCare.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.WhatsApp.Queries.GetWhatsAppSettings;

public class GetWhatsAppSettingsHandler : IRequestHandler<GetWhatsAppSettingsQuery, WhatsAppSettingsDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetWhatsAppSettingsHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<WhatsAppSettingsDto?> Handle(GetWhatsAppSettingsQuery request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUserService.OrganizationId;
        if (!organizationId.HasValue)
        {
            return null;
        }

        var settings = await _context.WhatsAppBusinessSettings
            .FirstOrDefaultAsync(
                s => s.OrganizationId == organizationId.Value && s.IsActive,
                cancellationToken);

        if (settings == null)
        {
            return null;
        }

        return new WhatsAppSettingsDto
        {
            Id = settings.Id,
            OrganizationId = settings.OrganizationId,
            IsEnabled = settings.IsEnabled,
            Provider = settings.Provider,
            PhoneNumberId = settings.PhoneNumberId,
            BusinessAccountId = settings.BusinessAccountId,
            ApiVersion = settings.ApiVersion,
            FromPhoneNumber = settings.FromPhoneNumber,
            WebhookUrl = settings.WebhookUrl,
            WebhookVerifyToken = settings.WebhookVerifyToken
        };
    }
}

