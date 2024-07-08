using System;
using System.Text;
using Sakur.WebApiUtilities.Models;

namespace WebApiUtilities.Helpers
{
    /// <summary>
    /// Will help with connection strings
    /// </summary>
    public static class ConnectionStringHelper
    {
        /// <summary>
        /// Will create a connection string from a url
        /// </summary>
        /// <param name="url"></param>
        /// <param name="sslMode"></param>
        /// <param name="trustServerCertificate"></param>
        /// <returns></returns>
        /// <exception cref="ApiException"></exception>
        public static string GetConnectionStringFromUrl(string url, SslMode sslMode = SslMode.Require, bool trustServerCertificate = true)
        {
            if (url == null)
                throw new ApiException("Url was null when trying to convert url to connection string", System.Net.HttpStatusCode.InternalServerError);

            try
            {
                Uri databaseUri = new Uri(url);
                string[] userInfo = databaseUri.UserInfo.Split(':');


                ConnectionStringBuilder builder = new ConnectionStringBuilder
                {
                    Host = databaseUri.Host,
                    Port = databaseUri.Port,
                    Username = userInfo[0],
                    Password = userInfo[1],
                    Database = databaseUri.LocalPath.TrimStart('/'),
                    SslMode = sslMode,
                    TrustServerCertificate = trustServerCertificate
                };

                return builder.ToString();
            }
            catch
            {
                throw new ApiException(new { message = "Unknown error when url was being converted to connection string", urlLength = url.Length }, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        private class ConnectionStringBuilder
        {
            public string? Host { get; set; }
            public int Port { get; set; }
            public string? Username { get; set; }
            public string? Password { get; set; }
            public string? Database { get; set; }
            public SslMode SslMode { get; set; }
            public bool TrustServerCertificate { get; set; }

            public ConnectionStringBuilder() { }

            public override string ToString()
            {
                StringBuilder stringBuilder = new StringBuilder();

                if (Host != null)
                {
                    stringBuilder.Append("Host=");
                    stringBuilder.Append(Host);
                    stringBuilder.Append(";");
                }

                if (Port != 0)
                {
                    stringBuilder.Append("Port=");
                    stringBuilder.Append(Port);
                    stringBuilder.Append(";");
                }

                if (Username != null)
                {
                    stringBuilder.Append("Username=");
                    stringBuilder.Append(Username);
                    stringBuilder.Append(";");
                }

                if (Password != null)
                {
                    stringBuilder.Append("Password=");
                    stringBuilder.Append(Password);
                    stringBuilder.Append(";");
                }

                if (Database != null)
                {
                    stringBuilder.Append("Database=");
                    stringBuilder.Append(Database);
                    stringBuilder.Append(";");
                }

                stringBuilder.Append("SSL Mode=");
                stringBuilder.Append(SslMode.ToString());
                stringBuilder.Append(";");

                stringBuilder.Append("Trust Server Certificate=");
                stringBuilder.Append(TrustServerCertificate);

                return stringBuilder.ToString();
            }
        }
    }

    /// <summary>
    /// What kind of ssl mode to use
    /// </summary>
    public enum SslMode
    {
        /// <summary>
        /// If ssl is disabled
        /// </summary>
        Disable,
        /// <summary>
        /// If ssl is preferred
        /// </summary>
        Prefer,
        /// <summary>
        /// If ssl is required
        /// </summary>
        Require
    }
}
