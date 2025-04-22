using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;
using WebApiUtilities.Models;

namespace WebApiUtilities.Filters
{
    /// <summary>
    /// Authorization filter that enforces API key validation on actions or controllers
    /// marked with the <see cref="RequireApiKeyAttribute"/>.
    /// </summary>
    public class ApiKeyFilter : IAuthorizationFilter
    {
        private readonly string _configuredKey;
        private const string HeaderName = "X-Api-Key";
        private const string EnvVariable = "API_KEY";

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiKeyFilter"/> class.
        /// </summary>
        public ApiKeyFilter()
        {
            _configuredKey = Environment.GetEnvironmentVariable(EnvVariable) ?? string.Empty;
        }

        /// <summary>
        /// Called to authorize a request based on the presence and validity of the API key.
        /// Only runs if the <see cref="RequireApiKeyAttribute"/> is applied.
        /// </summary>
        /// <param name="context">The authorization filter context.</param>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            bool hasAttribute = context.ActionDescriptor.EndpointMetadata
                .Any(meta => meta is RequireApiKeyAttribute);

            if (!hasAttribute)
            {
                return;
            }

            if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out StringValues providedKey) || _configuredKey != providedKey)
            {
                context.Result = new ContentResult
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Content = "Invalid or missing API key."
                };
            }
        }
    }
}
