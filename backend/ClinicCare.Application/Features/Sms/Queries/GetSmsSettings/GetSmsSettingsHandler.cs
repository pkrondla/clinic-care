using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Features.Sms.Queries.GetSmsSettings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.Sms.Queries.GetSmsSettings;

public class GetSmsSettingsHandler : IRequestHandler<GetSmsSettingsQuery, SmsSettingsDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDataProtectionService _dataProtection;

    public GetSmsSettingsHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IDataProtectionService dataProtection)
    {
        _context = context;
        _currentUserService = currentUserService;
        _dataProtection = dataProtection;
    }

    public async Task<SmsSettingsDto?> Handle(GetSmsSettingsQuery request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUserService.OrganizationId;
        if (!organizationId.HasValue)
        {
            return null;
        }

        var settings = await _context.SmsSettings
            .FirstOrDefaultAsync(
                s => s.OrganizationId == organizationId.Value && s.IsActive,
                cancellationToken);

        if (settings == null)
        {
            return null;
        }

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

