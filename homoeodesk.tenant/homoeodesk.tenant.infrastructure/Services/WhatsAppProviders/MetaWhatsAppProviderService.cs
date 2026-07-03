using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Services;
using HomoeoDesk.Tenant.Domain.Entities;
using HomoeoDesk.Tenant.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HomoeoDesk.Tenant.Infrastructure.Services.WhatsAppProviders;

/// <summary>
/// Meta WhatsApp Business API provider implementation
/// Documentation: https://developers.facebook.com/docs/whatsapp/cloud-api
/// </summary>
public class MetaWhatsAppProviderService : IWhatsAppProviderService
{
    private readonly WhatsAppBusinessSettings _settings;
    private readonly IDataProtectionService _dataProtection;
    private readonly HttpClient _httpClient;
    private readonly ILogger<MetaWhatsAppProviderService> _logger;
    private readonly string _apiVersion;
    private readonly string _phoneNumberId;
    private readonly string _accessToken;

    public MetaWhatsAppProviderService(
        WhatsAppBusinessSettings settings,
        IDataProtectionService dataProtection,
        IHttpClientFactory httpClientFactory,
        ILogger<MetaWhatsAppProviderService> logger)
    {
        _settings = settings;
        _dataProtection = dataProtection;
        _httpClient = httpClientFactory.CreateClient("MetaWhatsApp");
        _logger = logger;

        // Decrypt sensitive data
        _accessToken = !string.IsNullOrEmpty(settings.AccessToken) && _dataProtection.IsEncrypted(settings.AccessToken)
            ? _dataProtection.Decrypt(settings.AccessToken)
            : settings.AccessToken ?? throw new InvalidOperationException("AccessToken is required for Meta WhatsApp provider");

        _phoneNumberId = settings.PhoneNumberId ?? throw new InvalidOperationException("PhoneNumberId is required for Meta WhatsApp provider");
        _apiVersion = settings.ApiVersion ?? "v18.0";

        // Set authorization header
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);
    }

    public async Task<WhatsAppSendResult> SendTextMessageAsync(string to, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate phone number format (should be E.164: +1234567890)
            if (!IsValidPhoneNumber(to))
            {
                return new WhatsAppSendResult
                {
                    Success = false,
                    ErrorMessage = $"Invalid phone number format: {to}. Expected E.164 format (e.g., +1234567890)"
                };
            }

            var url = $"https://graph.facebook.com/{_apiVersion}/{_phoneNumberId}/messages";

            var payload = new
            {
                messaging_product = "whatsapp",
                to = to,
                type = "text",
                text = new
                {
                    body = message
                }
            };

            _logger.LogInformation("Sending WhatsApp text message to {To} via Meta API", to);

            var response = await _httpClient.PostAsJsonAsync(url, payload, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<MetaWhatsAppResponse>(cancellationToken: cancellationToken);
                
                _logger.LogInformation("WhatsApp message sent successfully. Message ID: {MessageId}", result?.Messages?[0]?.Id);

                return new WhatsAppSendResult
                {
                    Success = true,
                    MessageId = result?.Messages?[0]?.Id
                };
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to send WhatsApp message. Status: {Status}, Response: {Response}", 
                    response.StatusCode, errorContent);

                var errorResult = await response.Content.ReadFromJsonAsync<MetaWhatsAppErrorResponse>(cancellationToken: cancellationToken);
                
                return new WhatsAppSendResult
                {
                    Success = false,
                    ErrorMessage = errorResult?.Error?.Message ?? $"HTTP {response.StatusCode}: {errorContent}"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception sending WhatsApp text message to {To}", to);
            return new WhatsAppSendResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<WhatsAppSendResult> SendMediaMessageAsync(string to, byte[] media, string mediaType, string? caption = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate phone number format
            if (!IsValidPhoneNumber(to))
            {
                return new WhatsAppSendResult
                {
                    Success = false,
                    ErrorMessage = $"Invalid phone number format: {to}. Expected E.164 format (e.g., +1234567890)"
                };
            }

            // For media messages, we need to:
            // 1. Upload media to Meta's servers to get a media ID
            // 2. Send message with media ID

            // Step 1: Upload media
            var uploadUrl = $"https://graph.facebook.com/{_apiVersion}/{_phoneNumberId}/media";
            
            using var formData = new MultipartFormDataContent();
            formData.Add(new ByteArrayContent(media), "file", GetFileNameForMediaType(mediaType));
            formData.Add(new StringContent(mediaType), "type");
            formData.Add(new StringContent("whatsapp"), "messaging_product");

            _logger.LogInformation("Uploading media to Meta WhatsApp API for {To}", to);

            var uploadResponse = await _httpClient.PostAsync(uploadUrl, formData, cancellationToken);

            if (!uploadResponse.IsSuccessStatusCode)
            {
                var errorContent = await uploadResponse.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to upload media. Status: {Status}, Response: {Response}", 
                    uploadResponse.StatusCode, errorContent);
                
                return new WhatsAppSendResult
                {
                    Success = false,
                    ErrorMessage = $"Failed to upload media: {errorContent}"
                };
            }

            var uploadResult = await uploadResponse.Content.ReadFromJsonAsync<MetaWhatsAppMediaUploadResponse>(cancellationToken: cancellationToken);
            var mediaId = uploadResult?.Id;

            if (string.IsNullOrEmpty(mediaId))
            {
                return new WhatsAppSendResult
                {
                    Success = false,
                    ErrorMessage = "Media upload succeeded but no media ID returned"
                };
            }

            // Step 2: Send message with media ID
            var messageUrl = $"https://graph.facebook.com/{_apiVersion}/{_phoneNumberId}/messages";

            var payload = new
            {
                messaging_product = "whatsapp",
                to = to,
                type = GetMessageTypeForMediaType(mediaType),
                image = mediaType.StartsWith("image/") ? new { id = mediaId, caption = caption } : null,
                document = mediaType.StartsWith("application/") || mediaType.StartsWith("text/") ? new { id = mediaId, caption = caption } : null,
                video = mediaType.StartsWith("video/") ? new { id = mediaId, caption = caption } : null,
                audio = mediaType.StartsWith("audio/") ? new { id = mediaId } : null
            };

            _logger.LogInformation("Sending WhatsApp media message to {To} via Meta API", to);

            var response = await _httpClient.PostAsJsonAsync(messageUrl, payload, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<MetaWhatsAppResponse>(cancellationToken: cancellationToken);
                
                _logger.LogInformation("WhatsApp media message sent successfully. Message ID: {MessageId}", result?.Messages?[0]?.Id);

                return new WhatsAppSendResult
                {
                    Success = true,
                    MessageId = result?.Messages?[0]?.Id
                };
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to send WhatsApp media message. Status: {Status}, Response: {Response}", 
                    response.StatusCode, errorContent);

                var errorResult = await response.Content.ReadFromJsonAsync<MetaWhatsAppErrorResponse>(cancellationToken: cancellationToken);
                
                return new WhatsAppSendResult
                {
                    Success = false,
                    ErrorMessage = errorResult?.Error?.Message ?? $"HTTP {response.StatusCode}: {errorContent}"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception sending WhatsApp media message to {To}", to);
            return new WhatsAppSendResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<WhatsAppMessageStatus> GetMessageStatusAsync(string messageId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Meta WhatsApp API doesn't provide a direct endpoint to get message status
            // Status updates come via webhooks. For now, we'll return Pending.
            // In production, you'd query your database for the status from webhook updates.
            
            _logger.LogWarning("GetMessageStatusAsync not fully implemented for Meta provider. Message status should be tracked via webhooks.");
            return WhatsAppMessageStatus.Pending;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception getting message status for {MessageId}", messageId);
            return WhatsAppMessageStatus.Failed;
        }
    }

    public async Task<bool> ValidateConfigurationAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Test connection by getting phone number details
            var url = $"https://graph.facebook.com/{_apiVersion}/{_phoneNumberId}";

            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Meta WhatsApp configuration validated successfully");
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Meta WhatsApp configuration validation failed. Status: {Status}, Response: {Response}", 
                    response.StatusCode, errorContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception validating Meta WhatsApp configuration");
            return false;
        }
    }

    private bool IsValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        // E.164 format: + followed by country code and number (max 15 digits total)
        return System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, @"^\+[1-9]\d{1,14}$");
    }

    private string GetFileNameForMediaType(string mediaType)
    {
        return mediaType switch
        {
            var mt when mt.StartsWith("image/") => "image.jpg",
            var mt when mt.StartsWith("video/") => "video.mp4",
            var mt when mt.StartsWith("audio/") => "audio.mp3",
            var mt when mt.StartsWith("application/pdf") => "document.pdf",
            _ => "file"
        };
    }

    private string GetMessageTypeForMediaType(string mediaType)
    {
        return mediaType switch
        {
            var mt when mt.StartsWith("image/") => "image",
            var mt when mt.StartsWith("video/") => "video",
            var mt when mt.StartsWith("audio/") => "audio",
            _ => "document"
        };
    }

    // Meta API Response Models
    private class MetaWhatsAppResponse
    {
        [JsonPropertyName("messaging_product")]
        public string? MessagingProduct { get; set; }

        [JsonPropertyName("contacts")]
        public List<MetaContact>? Contacts { get; set; }

        [JsonPropertyName("messages")]
        public List<MetaMessage>? Messages { get; set; }
    }

    private class MetaContact
    {
        [JsonPropertyName("input")]
        public string? Input { get; set; }

        [JsonPropertyName("wa_id")]
        public string? WaId { get; set; }
    }

    private class MetaMessage
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
    }

    private class MetaWhatsAppErrorResponse
    {
        [JsonPropertyName("error")]
        public MetaError? Error { get; set; }
    }

    private class MetaError
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("code")]
        public int? Code { get; set; }
    }

    private class MetaWhatsAppMediaUploadResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
    }
}

