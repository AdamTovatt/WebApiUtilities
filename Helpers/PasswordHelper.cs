using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace WebApiUtilities.Helpers
{
    /// <summary>
    /// Password related tools
    /// </summary>
    public static class PasswordHelper
    {
        private static Random random = null;

        /// <summary>
        /// Will validate a password
        /// </summary>
        /// <param name="input">The input password that should be validated, in plaintext</param>
        /// <param name="dbPassword">The password hash and salt from the CreatePasswordHash function</param>
        /// <returns>A boolean telling wether tha passwords were a match or not</returns>
        public static bool ValidPassword(string input, string dbPassword)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(dbPassword))
                return false;

            byte[] hashBytes = Convert.FromBase64String(dbPassword);

            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            var pbkdf2 = new Rfc2898DeriveBytes(input, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);

            for (int i = 0; i < 20; i++)
                if (hashBytes[i + 16] != hash[i])
                    return false;

            return true;
        }

        /// <summary>
        /// Will create a hash and salt string from a plain-text password
        /// </summary>
        /// <param name="password">The password as plain-text</param>
        /// <returns>A string with a hash and salt from the plain-text password</returns>
        public static string CreatePasswordHash(string password)
        {
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);

            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Will create a new random password as a string
        /// </summary>
        /// <param name="length">The length of the password to create</param>
        /// <returns>A randomly generated password with the given length</returns>
        public static string CreateRandomPassword(int length)
        {
            if (random == null)
                random = new Random();

            List<byte> bytes = new List<byte>();

            for (int i = 0; i < length; i++)
            {
                if (random.Next(0, 100) > 50)
                    bytes.Add((byte)random.Next(65, 91));
                else
                    bytes.Add((byte)random.Next(97, 123));
            }

            return Encoding.ASCII.GetString(bytes.ToArray());
        }
    }
}
