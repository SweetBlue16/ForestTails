using System.Security.Cryptography;
using System.Text;

namespace ForestTails.Server.Logic.Utils
{
    public static class PasswordHelper
    {
        private const int KeySize = 64;
        private const int Iterations = 350000;
        private const char Delimiter = '.';

        private static readonly HashAlgorithmName hashAlgorithmName = HashAlgorithmName.SHA512;

        public static string HashPassword(string password)
        {
            var salt = SecureRandom.GenerateSalt(KeySize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password),
                salt, Iterations, hashAlgorithmName, KeySize
            );
            return string.Join(Delimiter, Iterations, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
        }

        public static bool VerifyPassword(string hashedPassword, string passwordInput)
        {
            if (string.IsNullOrWhiteSpace(hashedPassword) || string.IsNullOrWhiteSpace(passwordInput))
            {
                return false;
            }

            var parts = hashedPassword.Split(Delimiter);
            if (parts.Length != 3) return false;
            if (!int.TryParse(parts[0], out int iterations)) return false;

            var salt = Convert.FromBase64String(parts[1]);
            var originalHash = Convert.FromBase64String(parts[2]);

            var newHash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(passwordInput),
                salt, iterations, hashAlgorithmName, KeySize
            );
            return CryptographicOperations.FixedTimeEquals(originalHash, newHash);
        }
    }
}
