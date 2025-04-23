using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Distributed;
using System.Threading;
using System.Threading.Tasks;

namespace Sakur.WebApiUtilities.RateLimiting
{
    /// <summary>
    /// Contains extension methods related to rate limiting.
    /// </summary>
    public static class ExtensionMethods
    {
        internal async static Task<byte[]?> GetCachedValueAsync(this IDistributedCache cache, string key, CancellationToken token = default(CancellationToken))
        {
            byte[]? result = await cache.GetAsync(key, token);
            return result;
        }

        internal async static Task SetCachedValueAsync(this IDistributedCache cache, string key, byte[] value, CancellationToken token = default(CancellationToken))
        {
            await cache.SetAsync(key, value, token);
        }

        /// <summary>
        /// Will add the rate limiting middleware to the appilication builder.
        /// </summary>
        /// <param name="builder">The builder to add it to.</param>
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimitingMiddleware>();
        }
    }
}
