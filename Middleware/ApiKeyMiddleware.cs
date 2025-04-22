using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Threading.Tasks;

namespace WebApiUtilities.Middleware
{
    /// <summary>
    /// Middleware that validates API key passed in request headers.
    /// </summary>
    public class ApiKeyMiddleware
    {
        /// <summary>
        /// Name of the HTTP header used to pass the API key.
        /// </summary>
        public static string ApiKeyHeaderName { get; set; } = "X-Api-Key";

        /// <summary>
        /// Name of the environment variable that stores the configured API key.
        /// </summary>
        public static string ApiKeyEnvVariable { get; set; } = "API_KEY";

        private readonly RequestDelegate _next;
        private readonly string _configuredKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiKeyMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        public ApiKeyMiddleware(RequestDelegate next)
        {
            _next = next;
            _configuredKey = Environment.GetEnvironmentVariable(ApiKeyEnvVariable) ?? string.Empty;
        }

        /// <summary>
        /// Processes the HTTP request and checks for a valid API key.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out StringValues providedKey) || _configuredKey != providedKey)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid or missing API key.");
                return;
            }

            await _next(context);
        }
    }
}
