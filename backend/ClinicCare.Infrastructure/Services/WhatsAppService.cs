using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Services;
using Microsoft.Extensions.Logging;

namespace ClinicCare.Infrastructure.Services;

/// <summary>
/// Main WhatsApp service that delegates to provider-specific implementations
/// Uses WhatsAppProviderFactory to get the appropriate provider based on organization settings
/// </summary>
public class WhatsAppService : IWhatsAppService
{
    private readonly WhatsAppProviderFactory _providerFactory;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<WhatsAppService> _logger;

    public WhatsAppService(
        WhatsAppProviderFactory providerFactory,
        ICurrentUserService currentUserService,
        ILogger<WhatsAppService> logger)
    {
        _providerFactory = providerFactory;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<WhatsAppSendResult> SendTextMessageAsync(string to, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("WhatsApp: Sending text message to {To}", to);
        
        var organizationId = _currentUserService.OrganizationId;
        if (!organizationId.HasValue)
        {
            return new WhatsAppSendResult
            {
                Success = false,
                ErrorMessage = "User not associated with any organization"
            };
        }

        var provider = await _providerFactory.GetProviderAsync(organizationId.Value, cancellationToken);
        if (provider == null)
        {
            _logger.LogWarning("No WhatsApp provider configured for organization {OrganizationId}", organizationId.Value);
            return new WhatsAppSendResult
            {
                Success = false,
                ErrorMessage = "WhatsApp service not configured. Please configure a WhatsApp provider in settings."
            };
        }

        return await provider.SendTextMessageAsync(to, message, cancellationToken);
    }

    public async Task<WhatsAppSendResult> SendMediaMessageAsync(string to, byte[] media, string mediaType, string? caption = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("WhatsApp: Sending media message to {To}, Type: {MediaType}", to, mediaType);
        
        var organizationId = _currentUserService.OrganizationId;
        if (!organizationId.HasValue)
        {
            return new WhatsAppSendResult
            {
                Success = false,
                ErrorMessage = "User not associated with any organization"
            };
        }

        var provider = await _providerFactory.GetProviderAsync(organizationId.Value, cancellationToken);
        if (provider == null)
        {
            _logger.LogWarning("No WhatsApp provider configured for organization {OrganizationId}", organizationId.Value);
            return new WhatsAppSendResult
            {
                Success = false,
                ErrorMessage = "WhatsApp service not configured. Please configure a WhatsApp provider in settings."
            };
        }

        return await provider.SendMediaMessageAsync(to, media, mediaType, caption, cancellationToken);
    }

    public async Task<WhatsAppMessageStatus> GetMessageStatusAsync(string messageId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("WhatsApp: Getting message status for {MessageId}", messageId);
        
        var organizationId = _currentUserService.OrganizationId;
        if (!organizationId.HasValue)
        {
            _logger.LogWarning("User not associated with any organization");
            return WhatsAppMessageStatus.Failed;
        }

        var provider = await _providerFactory.GetProviderAsync(organizationId.Value, cancellationToken);
        if (provider == null)
        {
            _logger.LogWarning("No WhatsApp provider configured for organization {OrganizationId}", organizationId.Value);
            return WhatsAppMessageStatus.Failed;
        }

        return await provider.GetMessageStatusAsync(messageId, cancellationToken);
    }
}

