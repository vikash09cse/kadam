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

        public static (bool IsValid, string ErrorMessage) ValidatePasswordStrength(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return (false, "Password cannot be empty.");
            }

            if (password.Length < 6)
            {
                return (false, "Password must be at least 6 characters long.");
            }

            if (password.Length > 15)
            {
                return (false, "Password cannot exceed 15 characters.");
            }

            // Check for at least one uppercase letter
            if (!password.Any(char.IsUpper))
            {
                return (false, "Password must contain at least one uppercase letter.");
            }

            // Check for at least one lowercase letter
            if (!password.Any(char.IsLower))
            {
                return (false, "Password must contain at least one lowercase letter.");
            }

            // Check for at least one digit
            if (!password.Any(char.IsDigit))
            {
                return (false, "Password must contain at least one number.");
            }

            // Check for at least one special character
            var specialChars = "@$!%*?&";
            if (!password.Any(c => specialChars.Contains(c)))
            {
                return (false, "Password must contain at least one special character (@$!%*?&).");
            }

            // Check for common weak passwords
            var commonPasswords = new[] { "password", "123456", "qwerty", "abc123", "password123", "admin", "letmein" };
            if (commonPasswords.Any(weak => password.ToLower().Contains(weak.ToLower())))
            {
                return (false, "Password contains common weak patterns. Please choose a stronger password.");
            }

            return (true, string.Empty);
        }
    }
}
