using ClinicCare.Application.Common.Interfaces;
using System.Security.Cryptography;

namespace ClinicCare.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 32;
    private const int HashSize = 32;
    private const int Iterations = 10000;

    public string HashPassword(string password)
    {
        // Generate salt
        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[SaltSize];
        rng.GetBytes(salt);

        // Hash password with salt
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(HashSize);

        // Combine salt and hash
        var hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        try
        {
            // Support legacy format: "iterations:salt:hash" (e.g., "1000:XkB3Q2rT9/YvGZLKp5wF8A==:8vN2J5hK9mP4lR7sT3wA6bE=")
            if (hashedPassword.Contains(':'))
            {
                return VerifyLegacyPassword(password, hashedPassword);
            }

            // New format: Base64(salt + hash)
            var hashBytes = Convert.FromBase64String(hashedPassword);

            // Extract salt
            var salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            // Hash provided password with extracted salt
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(HashSize);

            // Compare hashes
            for (int i = 0; i < HashSize; i++)
            {
                if (hashBytes[i + SaltSize] != hash[i])
                    return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private bool VerifyLegacyPassword(string password, string hashedPassword)
    {
        try
        {
            var parts = hashedPassword.Split(':');
            if (parts.Length != 3)
                return false;

            var iterations = int.Parse(parts[0]);
            var saltBytes = Convert.FromBase64String(parts[1]);
            var storedHash = Convert.FromBase64String(parts[2]);

            // Hash provided password with extracted salt and iterations
            // Note: Legacy format might have used SHA1 instead of SHA256
            // Try SHA256 first, then fall back to SHA1 if that fails
            using var pbkdf2Sha256 = new Rfc2898DeriveBytes(password, saltBytes, iterations, HashAlgorithmName.SHA256);
            var computedHashSha256 = pbkdf2Sha256.GetBytes(storedHash.Length);

            // Compare hashes using constant-time comparison
            if (computedHashSha256.Length == storedHash.Length)
            {
                if (CryptographicOperations.FixedTimeEquals(computedHashSha256, storedHash))
                    return true;
            }

            // If SHA256 doesn't match, try SHA1 (legacy systems often used SHA1)
            using var pbkdf2Sha1 = new Rfc2898DeriveBytes(password, saltBytes, iterations, HashAlgorithmName.SHA1);
            var computedHashSha1 = pbkdf2Sha1.GetBytes(storedHash.Length);

            if (computedHashSha1.Length == storedHash.Length)
            {
                return CryptographicOperations.FixedTimeEquals(computedHashSha1, storedHash);
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}
