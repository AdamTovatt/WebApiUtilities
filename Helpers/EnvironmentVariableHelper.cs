using Sakur.WebApiUtilities.Models;
using System;
using System.Collections.Generic;

namespace WebApiUtilities.Helpers
{
    /// <summary>
    /// Will help with getting environment variables
    /// </summary>
    public static class EnvironmentHelper
    {
        private static Dictionary<string, string> variableCache = new Dictionary<string, string>();

        /// <summary>
        /// Will get an environment variable, throwing an exception if it is not found or too short
        /// </summary>
        /// <param name="environmentVariableName">The name of the environment variable</param>
        /// <param name="minLength">The minimum required lenght, default is 0</param>
        /// <param name="bypassCache">If the variable cache should be bypassed or not, default is false</param>
        /// <returns>The environment variable</returns>
        /// <exception cref="ApiException">If the environment variable doesn't exist or is too short</exception>
        public static string GetEnvironmentVariable(string environmentVariableName, int minLength = 0, bool bypassCache = false)
        {
            if (!bypassCache && variableCache.ContainsKey(environmentVariableName))
                return variableCache[environmentVariableName];

            string? result = Environment.GetEnvironmentVariable(environmentVariableName);

            if (result == null)
                throw new ApiException(string.Format("No {0} in environment variables", environmentVariableName), System.Net.HttpStatusCode.InternalServerError);
            if (result.Length < minLength)
                throw new ApiException(string.Format("Invalid {0} in environment variables", environmentVariableName), System.Net.HttpStatusCode.InternalServerError);

            if (!variableCache.ContainsKey(environmentVariableName))
                variableCache.Add(environmentVariableName, result);

            return result;
        }
    }
}
