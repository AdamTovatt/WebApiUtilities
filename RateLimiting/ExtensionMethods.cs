using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Distributed;
using System.Threading;
using System.Threading.Tasks;

namespace Sakur.WebApiUtilities.RateLimiting
{
    internal static class ExtensionMethods
    {
        public async static Task<byte[]?> GetCachedValueAsync(this IDistributedCache cache, string key, CancellationToken token = default(CancellationToken))
        {
            byte[]? result = await cache.GetAsync(key, token);
            return result;
        }

        public async static Task SetCachedValueAsync(this IDistributedCache cache, string key, byte[] value, CancellationToken token = default(CancellationToken))
        {
            await cache.SetAsync(key, value, token);
        }

        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimitingMiddleware>();
        }
    }
}
