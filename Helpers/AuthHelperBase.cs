using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Sakur.WebApiUtilities.Helpers
{
    /// <summary>
    /// Base class for handling authentication helpers, including generating and validating magic tokens and JWT tokens.
    /// </summary>
    /// <typeparam name="T">The derived type that extends this base class.</typeparam>
    public abstract class AuthHelperBase<T> where T : AuthHelperBase<T>, new()
    {
        private static T? instance;
        private static readonly object lockObj = new object();

        /// <summary>
        /// Singleton instance of the derived AuthHelper.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObj)
                    {
                        if (instance == null)
                        {
                            instance = new T();
                            instance.Initialize();
                        }
                    }
                }
                return instance;
            }
        }

        private string? magicLinkSecretKey;
        private string? jwtSecretKey;
        private string? issuer;
        private string? audience;
        private int magicLinkExpirationMinutes;
        private int jwtExpirationMinutes;

        /// <summary>
        /// Initializes the configuration settings. Must be overridden in the derived class.
        /// </summary>
        protected virtual void Initialize() { }

        /// <summary>
        /// Configures the authentication helper with the necessary settings.
        /// </summary>
        protected void Configure(
            string magicLinkSecretKey,
            string jwtSecretKey,
            string issuer,
            string audience,
            int magicLinkExpirationMinutes,
            int jwtExpirationMinutes)
        {
            this.magicLinkSecretKey = magicLinkSecretKey;
            this.jwtSecretKey = jwtSecretKey;
            this.issuer = issuer;
            this.audience = audience;
            this.magicLinkExpirationMinutes = magicLinkExpirationMinutes;
            this.jwtExpirationMinutes = jwtExpirationMinutes;
        }

        /// <summary>
        /// Generates a magic link token for the specified user ID.
        /// </summary>
        public string GenerateMagicToken(string userId)
        {
            if (magicLinkSecretKey == null)
                throw new InvalidOperationException("Magic link secret key has not been initialized.");

            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string payload = $"{userId}:{timestamp}";
            using HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(magicLinkSecretKey));
            string hash = ConvertToHex(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));
            return $"{ConvertToHex(Encoding.UTF8.GetBytes(payload))}.{hash}";
        }

        /// <summary>
        /// Validates a magic link token and extracts the user ID if valid.
        /// </summary>
        public bool ValidateMagicToken(string token, out string? userId)
        {
            userId = null;

            if (magicLinkSecretKey == null)
                throw new InvalidOperationException("Magic link secret key has not been initialized.");

            string[] parts = token.Split('.');
            if (parts.Length != 2) return false;

            string payload = Encoding.UTF8.GetString(ConvertFromHex(parts[0]));
            string receivedHash = parts[1];

            using HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(magicLinkSecretKey));
            string computedHash = ConvertToHex(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));

            if (computedHash != receivedHash) return false;

            string[] payloadParts = payload.Split(':');
            userId = payloadParts[0];
            long timestamp = long.Parse(payloadParts[1]);
            DateTimeOffset tokenTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);

            return DateTimeOffset.UtcNow <= tokenTime.AddMinutes(magicLinkExpirationMinutes);
        }

        /// <summary>
        /// Generates a JWT token for the specified user ID and permissions.
        /// </summary>
        public string GenerateJwtToken(string userId, IEnumerable<string> permissions)
        {
            if (jwtSecretKey == null || issuer == null || audience == null)
                throw new InvalidOperationException("JWT configuration has not been fully initialized.");

            List<Claim> claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("permissions", string.Join(' ', permissions))
            };

            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey));
            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwtExpirationMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Converts a byte array to a hexadecimal string.
        /// </summary>
        private static string ConvertToHex(byte[] bytes)
        {
            StringBuilder hex = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        /// <summary>
        /// Converts a hexadecimal string to a byte array.
        /// </summary>
        private static byte[] ConvertFromHex(string hex)
        {
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            return bytes;
        }
    }
}
