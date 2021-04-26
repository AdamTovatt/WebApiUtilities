using System;
using Sakur.WebApiUtilities.Models;

namespace WebApiUtilities.Helpers
{
    public static class ConnectionStringHelper
    {
        public static string GetConnectionStringFromUrl(string url)
        {
            if (url == null)
                throw new ApiException("Url was null when trying to convert url to connection string", System.Net.HttpStatusCode.InternalServerError);

            try
            {
                Uri databaseUri = new Uri(url);
                string[] userInfo = databaseUri.UserInfo.Split(':');

                /*
                string connectionString = "Server={0}; Database={1}; User Id={2}; Password={3}; Database={4}; "

                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
                {
                    Host = databaseUri.Host,
                    Port = databaseUri.Port,
                    Username = userInfo[0],
                    Password = userInfo[1],
                    Database = databaseUri.LocalPath.TrimStart('/'),
                    SslMode = SslMode.Require,
                    TrustServerCertificate = true
                };
                

                return builder.ToString();
                */
                return null;
            }
            catch
            {
                throw new ApiException(new { message = "Unknown error when url was being converted to connection string", urlLength = url.Length }, System.Net.HttpStatusCode.InternalServerError);
            }
        }
    }
}
}
