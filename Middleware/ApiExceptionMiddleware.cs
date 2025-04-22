using Microsoft.AspNetCore.Http;
using Sakur.WebApiUtilities.Models;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebApiUtilities.Middleware
{
    /// <summary>
    /// Middleware that handles unhandled exceptions and returns a formatted JSON response.
    /// </summary>
    public class ApiExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiExceptionMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        public ApiExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Processes the HTTP request and handles any unhandled exceptions.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                ApiResponse response = exception is ApiException apiException
                    ? new ApiResponse(apiException)
                    : new ApiResponse(exception.Message, HttpStatusCode.InternalServerError);

                string json = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(json);
            }
        }
    }
}
