namespace ClinicCare.Application.Common.Services;

/// <summary>
/// Provider-agnostic interface for WhatsApp messaging services
/// Implementations: MetaWhatsAppProviderService, TwilioWhatsAppProviderService, etc.
/// </summary>
public interface IWhatsAppProviderService
{
    /// <summary>
    /// Send a text message via WhatsApp
    /// </summary>
    /// <param name="to">Recipient phone number (E.164 format: +1234567890)</param>
    /// <param name="message">Message text</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing message ID and success status</returns>
    Task<WhatsAppSendResult> SendTextMessageAsync(
        string to, 
        string message, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a media message (image, PDF, etc.) via WhatsApp
    /// </summary>
    /// <param name="to">Recipient phone number (E.164 format)</param>
    /// <param name="media">Media file bytes</param>
    /// <param name="mediaType">MIME type (e.g., "image/jpeg", "application/pdf")</param>
    /// <param name="caption">Optional caption text</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing message ID and success status</returns>
    Task<WhatsAppSendResult> SendMediaMessageAsync(
        string to, 
        byte[] media, 
        string mediaType, 
        string? caption = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the status of a sent message
    /// </summary>
    /// <param name="messageId">Provider-specific message ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Message status (Sent, Delivered, Read, Failed, etc.)</returns>
    Task<WhatsAppMessageStatus> GetMessageStatusAsync(
        string messageId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate provider configuration (test connection)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if configuration is valid and connection works</returns>
    Task<bool> ValidateConfigurationAsync(CancellationToken cancellationToken = default);
}

