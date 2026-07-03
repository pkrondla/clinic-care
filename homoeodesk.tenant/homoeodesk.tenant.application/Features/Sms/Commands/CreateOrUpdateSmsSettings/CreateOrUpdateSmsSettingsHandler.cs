using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Features.Sms.Queries.GetSmsSettings;
using HomoeoDesk.Tenant.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Sms.Commands.CreateOrUpdateSmsSettings;

public class CreateOrUpdateSmsSettingsHandler : IRequestHandler<CreateOrUpdateSmsSettingsCommand, SmsSettingsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDataProtectionService _dataProtection;

    public CreateOrUpdateSmsSettingsHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IDataProtectionService dataProtection)
    {
        _context = context;
        _currentUserService = currentUserService;
        _dataProtection = dataProtection;
    }

    public async Task<SmsSettingsDto> Handle(CreateOrUpdateSmsSettingsCommand request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUserService.OrganizationId;
        if (!organizationId.HasValue)
        {
            throw new UnauthorizedAccessException("User not associated with any organization");
        }

        var existingSettings = await _context.SmsSettings
            .FirstOrDefaultAsync(
                s => s.OrganizationId == organizationId.Value && s.IsActive,
                cancellationToken);

        SmsSettings settings;

        if (existingSettings != null)
        {
            // Update existing settings
            settings = existingSettings;
            settings.IsEnabled = request.IsEnabled;
            settings.Provider = request.Provider;
            settings.AccountSid = request.AccountSid;
            settings.FromPhoneNumber = request.FromPhoneNumber;
            settings.SenderId = request.SenderId;
            settings.ApiUrl = request.ApiUrl;
            settings.TimeoutSeconds = request.TimeoutSeconds ?? 30;
            settings.UpdatedAt = DateTime.UtcNow;

            // Encrypt sensitive fields if provided
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

            if (!string.IsNullOrEmpty(request.AuthToken))
            {
                settings.AuthToken = _dataProtection.IsEncrypted(request.AuthToken)
                    ? request.AuthToken
                    : _dataProtection.Encrypt(request.AuthToken);
            }
        }
        else
        {
            // Create new settings
            settings = new SmsSettings
            {
                OrganizationId = organizationId.Value,
                IsEnabled = request.IsEnabled,
                Provider = request.Provider,
                ApiKey = !string.IsNullOrEmpty(request.ApiKey)
                    ? _dataProtection.Encrypt(request.ApiKey)
                    : null,
                ApiSecret = !string.IsNullOrEmpty(request.ApiSecret)
                    ? _dataProtection.Encrypt(request.ApiSecret)
                    : null,
                AccountSid = request.AccountSid,
                AuthToken = !string.IsNullOrEmpty(request.AuthToken)
                    ? _dataProtection.Encrypt(request.AuthToken)
                    : null,
                FromPhoneNumber = request.FromPhoneNumber,
                SenderId = request.SenderId,
                ApiUrl = request.ApiUrl,
                TimeoutSeconds = request.TimeoutSeconds ?? 30,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.SmsSettings.Add(settings);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new SmsSettingsDto
        {
            Id = settings.Id,
            OrganizationId = settings.OrganizationId,
            IsEnabled = settings.IsEnabled,
            Provider = settings.Provider,
            ApiKey = !string.IsNullOrEmpty(settings.ApiKey)
                ? _dataProtection.Decrypt(settings.ApiKey)
                : null,
            ApiSecret = !string.IsNullOrEmpty(settings.ApiSecret)
                ? _dataProtection.Decrypt(settings.ApiSecret)
                : null,
            AccountSid = settings.AccountSid,
            AuthToken = !string.IsNullOrEmpty(settings.AuthToken)
                ? _dataProtection.Decrypt(settings.AuthToken)
                : null,
            FromPhoneNumber = settings.FromPhoneNumber,
            SenderId = settings.SenderId,
            ApiUrl = settings.ApiUrl,
            TimeoutSeconds = settings.TimeoutSeconds
        };
    }
}

