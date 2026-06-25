using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Features.WhatsApp.Queries.GetWhatsAppSettings;
using ClinicCare.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.WhatsApp.Commands.CreateOrUpdateWhatsAppSettings;

public class CreateOrUpdateWhatsAppSettingsHandler : IRequestHandler<CreateOrUpdateWhatsAppSettingsCommand, WhatsAppSettingsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDataProtectionService _dataProtection;

    public CreateOrUpdateWhatsAppSettingsHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IDataProtectionService dataProtection)
    {
        _context = context;
        _currentUserService = currentUserService;
        _dataProtection = dataProtection;
    }

    public async Task<WhatsAppSettingsDto> Handle(CreateOrUpdateWhatsAppSettingsCommand request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUserService.OrganizationId;
        if (!organizationId.HasValue)
        {
            throw new UnauthorizedAccessException("User not associated with any organization");
        }

        // Check if settings already exist
        var existingSettings = await _context.WhatsAppBusinessSettings
            .FirstOrDefaultAsync(
                s => s.OrganizationId == organizationId.Value && s.IsActive,
                cancellationToken);

        WhatsAppBusinessSettings settings;

        if (existingSettings != null)
        {
            // Update existing settings
            settings = existingSettings;
            settings.IsEnabled = request.IsEnabled;
            settings.Provider = request.Provider;
            settings.PhoneNumberId = request.PhoneNumberId;
            settings.BusinessAccountId = request.BusinessAccountId;
            settings.ApiVersion = request.ApiVersion ?? "v18.0";
            settings.FromPhoneNumber = request.FromPhoneNumber;
            settings.WebhookUrl = request.WebhookUrl;
            settings.WebhookVerifyToken = request.WebhookVerifyToken;
            settings.UpdatedAt = DateTime.UtcNow;

            // Encrypt sensitive fields if provided and not already encrypted
            if (!string.IsNullOrEmpty(request.AccessToken))
            {
                settings.AccessToken = _dataProtection.IsEncrypted(request.AccessToken)
                    ? request.AccessToken
                    : _dataProtection.Encrypt(request.AccessToken);
            }

            if (!string.IsNullOrEmpty(request.ApiKey))
            {
                settings.ApiKey = _dataProtection.IsEncrypted(request.ApiKey)
                    ? request.ApiKey
                    : _dataProtection.Encrypt(request.ApiKey);
            }

            if (!string.IsNullOrEmpty(request.ApiSecret))
            {
                settings.ApiSecret = _dataProtection.IsEncrypted(request.ApiSecret)
                    ? request.ApiSecret
                    : _dataProtection.Encrypt(request.ApiSecret);
            }

            if (!string.IsNullOrEmpty(request.WebhookSecret))
            {
                settings.WebhookSecret = _dataProtection.IsEncrypted(request.WebhookSecret)
                    ? request.WebhookSecret
                    : _dataProtection.Encrypt(request.WebhookSecret);
            }
        }
        else
        {
            // Create new settings
            settings = new WhatsAppBusinessSettings
            {
                OrganizationId = organizationId.Value,
                IsEnabled = request.IsEnabled,
                Provider = request.Provider,
                PhoneNumberId = request.PhoneNumberId,
                BusinessAccountId = request.BusinessAccountId,
                ApiVersion = request.ApiVersion ?? "v18.0",
                FromPhoneNumber = request.FromPhoneNumber,
                WebhookUrl = request.WebhookUrl,
                WebhookVerifyToken = request.WebhookVerifyToken,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Encrypt sensitive fields
            if (!string.IsNullOrEmpty(request.AccessToken))
            {
                settings.AccessToken = _dataProtection.Encrypt(request.AccessToken);
            }

            if (!string.IsNullOrEmpty(request.ApiKey))
            {
                settings.ApiKey = _dataProtection.Encrypt(request.ApiKey);
            }

            if (!string.IsNullOrEmpty(request.ApiSecret))
            {
                settings.ApiSecret = _dataProtection.Encrypt(request.ApiSecret);
            }

            if (!string.IsNullOrEmpty(request.WebhookSecret))
            {
                settings.WebhookSecret = _dataProtection.Encrypt(request.WebhookSecret);
            }

            _context.WhatsAppBusinessSettings.Add(settings);
        }

        await _context.SaveChangesAsync(cancellationToken);

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

