using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utilities
{
    public static class PasswordManagement
    {
        public static (byte[] Hash, byte[] Salt) HashPassword(string password)
        {
            byte[] salt;
            byte[] passwordHash;

            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                salt = new byte[32];
                rng.GetBytes(salt);

                using (var sha256 = System.Security.Cryptography.SHA256.Create())
                {
                    var passwordBytes = Encoding.UTF8.GetBytes(password);
                    var passwordWithSalt = new byte[passwordBytes.Length + salt.Length];
                    Buffer.BlockCopy(passwordBytes, 0, passwordWithSalt, 0, passwordBytes.Length);
                    Buffer.BlockCopy(salt, 0, passwordWithSalt, passwordBytes.Length, salt.Length);

                    passwordHash = sha256.ComputeHash(passwordWithSalt);
                }
            }

            return (passwordHash, salt);
        }

        public static bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var passwordBytes = Encoding.UTF8.GetBytes(password);
                var passwordWithSalt = new byte[passwordBytes.Length + storedSalt.Length];
                Buffer.BlockCopy(passwordBytes, 0, passwordWithSalt, 0, passwordBytes.Length);
                Buffer.BlockCopy(storedSalt, 0, passwordWithSalt, passwordBytes.Length, storedSalt.Length);

                var computedHash = sha256.ComputeHash(passwordWithSalt);
                return computedHash.SequenceEqual(storedHash);
            }
        }

        public static string GenerateRandomPassword(int minLength, int maxLength)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            int length = random.Next(minLength, maxLength + 1);
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
