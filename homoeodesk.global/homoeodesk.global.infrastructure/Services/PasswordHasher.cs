using HomoeoDesk.Global.Application.Common.Interfaces;
using System.Security.Cryptography;

namespace HomoeoDesk.Global.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 32;
    private const int HashSize = 32;
    private const int Iterations = 10000;

    public string HashPassword(string password)
    {
        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[SaltSize];
        rng.GetBytes(salt);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(HashSize);

        var hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        try
        {
            if (hashedPassword.Contains(':'))
            {
                return VerifyLegacyPassword(password, hashedPassword);
            }

            var hashBytes = Convert.FromBase64String(hashedPassword);
            var salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(HashSize);

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

    private static bool VerifyLegacyPassword(string password, string hashedPassword)
    {
        try
        {
            var parts = hashedPassword.Split(':');
            if (parts.Length != 3)
                return false;

            var iterations = int.Parse(parts[0]);
            var saltBytes = Convert.FromBase64String(parts[1]);
            var storedHash = Convert.FromBase64String(parts[2]);

            using var pbkdf2Sha256 = new Rfc2898DeriveBytes(password, saltBytes, iterations, HashAlgorithmName.SHA256);
            var computedHashSha256 = pbkdf2Sha256.GetBytes(storedHash.Length);

            if (computedHashSha256.Length == storedHash.Length &&
                CryptographicOperations.FixedTimeEquals(computedHashSha256, storedHash))
            {
                return true;
            }

            using var pbkdf2Sha1 = new Rfc2898DeriveBytes(password, saltBytes, iterations, HashAlgorithmName.SHA1);
            var computedHashSha1 = pbkdf2Sha1.GetBytes(storedHash.Length);

            return computedHashSha1.Length == storedHash.Length &&
                   CryptographicOperations.FixedTimeEquals(computedHashSha1, storedHash);
        }
        catch
        {
            return false;
        }
    }
}
