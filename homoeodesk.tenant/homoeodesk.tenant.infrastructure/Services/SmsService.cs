using HomoeoDesk.Tenant.Application.Common.Services;
using Microsoft.Extensions.Logging;

namespace HomoeoDesk.Tenant.Infrastructure.Services;

/// <summary>
/// Base implementation of SMS service
/// This is a placeholder implementation that can be extended with specific providers (Twilio, AWS SNS, etc.)
/// </summary>
public class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;

    public SmsService(ILogger<SmsService> logger)
    {
        _logger = logger;
    }

    public Task<SmsSendResult> SendSmsAsync(string to, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("SMS: Sending message to {To}", to);
        
        // TODO: Implement actual SMS provider integration
        // This is a placeholder - replace with actual provider (Twilio, AWS SNS, etc.)
        
        return Task.FromResult(new SmsSendResult
        {
            Success = false,
            ErrorMessage = "SMS service not configured. Please configure an SMS provider."
        });
    }

    public Task<SmsMessageStatus> GetMessageStatusAsync(string messageId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("SMS: Getting message status for {MessageId}", messageId);
        
        // TODO: Implement actual SMS provider integration
        
        return Task.FromResult(SmsMessageStatus.Pending);
    }
}

