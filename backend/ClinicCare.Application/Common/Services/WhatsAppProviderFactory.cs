using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Domain.Entities;
using ClinicCare.Domain.Enums;
using ClinicCare.Infrastructure.Services.WhatsAppProviders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace ClinicCare.Application.Common.Services;

/// <summary>
/// Factory for creating WhatsApp provider service instances based on organization settings
/// </summary>
public class WhatsAppProviderFactory
{
    private readonly IApplicationDbContext _context;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WhatsAppProviderFactory> _logger;

    public WhatsAppProviderFactory(
        IApplicationDbContext context,
        IServiceProvider serviceProvider,
        ILogger<WhatsAppProviderFactory> logger)
    {
        _context = context;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Get the appropriate WhatsApp provider service for an organization
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>WhatsApp provider service instance, or null if not configured</returns>
    public async Task<IWhatsAppProviderService?> GetProviderAsync(
        int organizationId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Load WhatsApp settings for the organization
            var settings = await _context.WhatsAppBusinessSettings
                .FirstOrDefaultAsync(
                    s => s.OrganizationId == organizationId 
                      && s.IsActive 
                      && s.IsEnabled, 
                    cancellationToken);

            if (settings == null)
            {
                _logger.LogDebug("No active WhatsApp settings found for organization {OrganizationId}", organizationId);
                return null;
            }

            // Create provider instance based on provider type
            return settings.Provider switch
            {
                WhatsAppProvider.Meta => CreateMetaProvider(settings),
                WhatsAppProvider.Twilio => CreateTwilioProvider(settings),
                WhatsAppProvider.Dialog360 => CreateDialog360Provider(settings),
                _ => null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting WhatsApp provider for organization {OrganizationId}", organizationId);
            return null;
        }
    }

    private IWhatsAppProviderService? CreateMetaProvider(WhatsAppBusinessSettings settings)
    {
        try
        {
            var dataProtection = _serviceProvider.GetRequiredService<IDataProtectionService>();
            var httpClientFactory = _serviceProvider.GetRequiredService<IHttpClientFactory>();
            var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<MetaWhatsAppProviderService>();

            return new MetaWhatsAppProviderService(settings, dataProtection, httpClientFactory, logger);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Meta WhatsApp provider");
            return null;
        }
    }

    private IWhatsAppProviderService? CreateTwilioProvider(WhatsAppBusinessSettings settings)
    {
        // Twilio provider will be implemented later
        _logger.LogWarning("Twilio WhatsApp provider not yet implemented");
        return null;
    }

    private IWhatsAppProviderService? CreateDialog360Provider(WhatsAppBusinessSettings settings)
    {
        // Dialog360 provider will be implemented later
        _logger.LogWarning("Dialog360 WhatsApp provider not yet implemented");
        return null;
    }
}

