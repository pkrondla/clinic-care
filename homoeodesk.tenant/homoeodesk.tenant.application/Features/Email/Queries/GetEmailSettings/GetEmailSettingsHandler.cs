using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Features.Email.Queries.GetEmailSettings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Email.Queries.GetEmailSettings;

public class GetEmailSettingsHandler : IRequestHandler<GetEmailSettingsQuery, EmailSettingsDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDataProtectionService _dataProtection;

    public GetEmailSettingsHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IDataProtectionService dataProtection)
    {
        _context = context;
        _currentUserService = currentUserService;
        _dataProtection = dataProtection;
    }

    public async Task<EmailSettingsDto?> Handle(GetEmailSettingsQuery request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUserService.OrganizationId;
        if (!organizationId.HasValue)
        {
            return null;
        }

        var settings = await _context.EmailSettings
            .FirstOrDefaultAsync(
                s => s.OrganizationId == organizationId.Value && s.IsActive,
                cancellationToken);

        if (settings == null)
        {
            return null;
        }

        return new EmailSettingsDto
        {
            Id = settings.Id,
            OrganizationId = settings.OrganizationId,
            IsEnabled = settings.IsEnabled,
            SmtpServer = settings.SmtpServer,
            SmtpPort = settings.SmtpPort,
            UseSsl = settings.UseSsl,
            UseTls = settings.UseTls,
            SmtpUsername = settings.SmtpUsername,
            SmtpPassword = !string.IsNullOrEmpty(settings.SmtpPassword) 
                ? _dataProtection.Decrypt(settings.SmtpPassword) 
                : null,
            FromEmail = settings.FromEmail,
            FromName = settings.FromName,
            ReplyToEmail = settings.ReplyToEmail,
            TimeoutSeconds = settings.TimeoutSeconds
        };
    }
}

