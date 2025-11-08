using System.Security.Cryptography;
using System.Text;

namespace PasswordHashGenerator
{
    class Program
    {
        private const int SaltSize = 32;
        private const int HashSize = 32;
        private const int Iterations = 10000;

        static void Main(string[] args)
        {
            Console.WriteLine("🔐 ClinicCare Password Hash Generator (PBKDF2)");
            Console.WriteLine("=============================================\n");

            // Demo passwords for development
            var demoPasswords = new Dictionary<string, string>
            {
                { "admin@healthcareplus.com", "Admin@123" },
                { "dr.smith@healthcareplus.com", "Doctor@123" },
                { "dr.johnson@healthcareplus.com", "Doctor@123" },
                { "dr.williams@healthcareplus.com", "Doctor@123" },
                { "reception1@healthcareplus.com", "Staff@123" },
                { "reception2@healthcareplus.com", "Staff@123" },
                { "pharmacy1@healthcareplus.com", "Staff@123" },
                { "patient1@email.com", "Patient@123" },
                { "patient2@email.com", "Patient@123" },
                { "patient3@email.com", "Patient@123" },
                { "patient4@email.com", "Patient@123" },
                { "patient5@email.com", "Patient@123" }
            };

            Console.WriteLine("Generated Password Hashes for Database (PBKDF2):");
            Console.WriteLine("================================================\n");

            foreach (var user in demoPasswords)
            {
                string hash = HashPassword(user.Value);
                Console.WriteLine($"-- User: {user.Key}");
                Console.WriteLine($"-- Password: {user.Value}");
                Console.WriteLine($"-- Hash: {hash}");
                Console.WriteLine();
            }

            Console.WriteLine("\n📋 SQL Update Statements:");
            Console.WriteLine("========================\n");

            foreach (var user in demoPasswords)
            {
                string hash = HashPassword(user.Value);
                Console.WriteLine($"UPDATE Users SET PasswordHash = '{hash}' WHERE Email = '{user.Key}';");
            }

            Console.WriteLine("\n✅ Password hashes generated successfully!");
            Console.WriteLine("💡 Copy the UPDATE statements above to update your database.");
        }

        static string HashPassword(string password)
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
    }
}