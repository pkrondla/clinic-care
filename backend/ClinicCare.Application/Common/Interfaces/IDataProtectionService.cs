namespace ClinicCare.Application.Common.Interfaces;

/// <summary>
/// Service for encrypting and decrypting sensitive data
/// Uses .NET Data Protection API
/// </summary>
public interface IDataProtectionService
{
    /// <summary>
    /// Encrypt a string value
    /// </summary>
    string Encrypt(string plainText);

    /// <summary>
    /// Decrypt an encrypted string value
    /// </summary>
    string Decrypt(string encryptedText);

    /// <summary>
    /// Check if a string is encrypted (heuristic check)
    /// </summary>
    bool IsEncrypted(string value);
}

