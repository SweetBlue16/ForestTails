using System.Security.Cryptography;

namespace ForestTails.Server.Logic.Utils
{
    public static class SecureRandom
    {
        private const string AlphanumericChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

        public static string GenerateCode(int length)
        {
            return RandomNumberGenerator.GetString(AlphanumericChars, length);
        }

        public static byte[] GenerateSalt(int size = 32)
        {
            var salt = new byte[size];
            RandomNumberGenerator.Fill(salt);
            return salt;
        }

        public static int Next(int min, int max)
        {
            return RandomNumberGenerator.GetInt32(min, max);
        }
    }
}
