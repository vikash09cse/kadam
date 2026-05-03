using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

        /// <summary>
        /// Generates a cryptographically random password meeting <see cref="ValidatePasswordStrength"/> rules.
        /// </summary>
        public static string GenerateSecurePassword(int length = 12)
        {
            length = Math.Clamp(length, 8, 15);
            const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string lower = "abcdefghjkmnpqrstuvwxyz";
            const string digits = "23456789";
            const string special = "@$!%*?&";

            Span<char> pwd = stackalloc char[length];
            var pool = upper + lower + digits + special;
            pwd[0] = upper[RandomNumberGenerator.GetInt32(upper.Length)];
            pwd[1] = lower[RandomNumberGenerator.GetInt32(lower.Length)];
            pwd[2] = digits[RandomNumberGenerator.GetInt32(digits.Length)];
            pwd[3] = special[RandomNumberGenerator.GetInt32(special.Length)];
            for (int i = 4; i < length; i++)
                pwd[i] = pool[RandomNumberGenerator.GetInt32(pool.Length)];

            // Fisher–Yates shuffle
            for (int i = length - 1; i > 0; i--)
            {
                int j = RandomNumberGenerator.GetInt32(i + 1);
                (pwd[i], pwd[j]) = (pwd[j], pwd[i]);
            }

            return new string(pwd);
        }

        public static string GenerateRandomPassword(int minLength, int maxLength)
        {
            return GenerateSecurePassword(Math.Clamp((minLength + maxLength) / 2, minLength, maxLength));
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
