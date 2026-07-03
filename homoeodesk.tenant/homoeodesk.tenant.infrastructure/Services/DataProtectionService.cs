using HomoeoDesk.Tenant.Application.Common.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;

namespace HomoeoDesk.Tenant.Infrastructure.Services;

/// <summary>
/// Implementation of data protection service using .NET Data Protection API
/// </summary>
public class DataProtectionService : IDataProtectionService
{
    private readonly IDataProtector _protector;
    private readonly ILogger<DataProtectionService> _logger;
    private const string Purpose = "HomoeoDesk.WhatsApp.SensitiveData";

    public DataProtectionService(
        IDataProtectionProvider dataProtectionProvider,
        ILogger<DataProtectionService> logger)
    {
        _protector = dataProtectionProvider.CreateProtector(Purpose);
        _logger = logger;
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        try
        {
            return _protector.Protect(plainText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting data");
            throw;
        }
    }

    public string Decrypt(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
            return encryptedText;

        try
        {
            return _protector.Unprotect(encryptedText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting data");
            throw;
        }
    }

    public bool IsEncrypted(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        // Heuristic: Encrypted strings from Data Protection API are base64-like
        // and typically start with specific patterns. This is a simple check.
        // A more robust check would try to decrypt and catch exceptions.
        try
        {
            _protector.Unprotect(value);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

