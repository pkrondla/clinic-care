using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Features.Email.Queries.GetEmailSettings;
using ClinicCare.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.Email.Commands.CreateOrUpdateEmailSettings;

public class CreateOrUpdateEmailSettingsHandler : IRequestHandler<CreateOrUpdateEmailSettingsCommand, EmailSettingsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDataProtectionService _dataProtection;

    public CreateOrUpdateEmailSettingsHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IDataProtectionService dataProtection)
    {
        _context = context;
        _currentUserService = currentUserService;
        _dataProtection = dataProtection;
    }

    public async Task<EmailSettingsDto> Handle(CreateOrUpdateEmailSettingsCommand request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUserService.OrganizationId;
        if (!organizationId.HasValue)
        {
            throw new UnauthorizedAccessException("User not associated with any organization");
        }

        var existingSettings = await _context.EmailSettings
            .FirstOrDefaultAsync(
                s => s.OrganizationId == organizationId.Value && s.IsActive,
                cancellationToken);

        EmailSettings settings;

        if (existingSettings != null)
        {
            // Update existing settings
            settings = existingSettings;
            settings.IsEnabled = request.IsEnabled;
            settings.SmtpServer = request.SmtpServer;
            settings.SmtpPort = request.SmtpPort ?? 587;
            settings.UseSsl = request.UseSsl;
            settings.UseTls = request.UseTls;
            settings.SmtpUsername = request.SmtpUsername;
            settings.FromEmail = request.FromEmail;
            settings.FromName = request.FromName;
            settings.ReplyToEmail = request.ReplyToEmail;
            settings.TimeoutSeconds = request.TimeoutSeconds ?? 30;
            settings.UpdatedAt = DateTime.UtcNow;

            // Encrypt password if provided
            if (!string.IsNullOrEmpty(request.SmtpPassword))
            {
                settings.SmtpPassword = _dataProtection.IsEncrypted(request.SmtpPassword)
                    ? request.SmtpPassword
                    : _dataProtection.Encrypt(request.SmtpPassword);
            }
        }
        else
        {
            // Create new settings
            settings = new EmailSettings
            {
                OrganizationId = organizationId.Value,
                IsEnabled = request.IsEnabled,
                SmtpServer = request.SmtpServer,
                SmtpPort = request.SmtpPort ?? 587,
                UseSsl = request.UseSsl,
                UseTls = request.UseTls,
                SmtpUsername = request.SmtpUsername,
                SmtpPassword = !string.IsNullOrEmpty(request.SmtpPassword)
                    ? _dataProtection.Encrypt(request.SmtpPassword)
                    : null,
                FromEmail = request.FromEmail,
                FromName = request.FromName,
                ReplyToEmail = request.ReplyToEmail,
                TimeoutSeconds = request.TimeoutSeconds ?? 30,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.EmailSettings.Add(settings);
        }

        await _context.SaveChangesAsync(cancellationToken);

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

