using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Sakur.WebApiUtilities.RateLimiting
{
    /// <summary>
    /// Used to limit the amount of requests a client can make to an endpoint
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IDistributedCache cache;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="next"></param>
        /// <param name="cache"></param>
        public RateLimitingMiddleware(RequestDelegate next, IDistributedCache cache)
        {
            this.next = next;
            this.cache = cache;
        }

        /// <summary>
        /// Invokes the middleware
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            Endpoint? endpoint = context.GetEndpoint();

            Limit? limit = null;

            if (endpoint != null)
                limit = endpoint.Metadata.GetMetadata<Limit>();

            if (limit is null)
            {
                await next(context);
                return;
            }

            string key = GenerateClientKey(context);
            ClientStatistics? clientStatistics = GetClientStatisticsByKey(key).Result;

            if (clientStatistics != null && clientStatistics.Count > 0)
            {
                if (DateTime.UtcNow < clientStatistics.First().AddSeconds(limit.TimeWindow) && clientStatistics.Count == limit.MaxRequests)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                    return;
                }
            }

            await UpdateClientStatisticsAsync(clientStatistics, key, limit.MaxRequests);
            await next(context);
        }

        private static string GenerateClientKey(HttpContext context)
        {
            return $"{context.Request.Path}_{context.Connection.RemoteIpAddress}";
        }

        private async Task<ClientStatistics?> GetClientStatisticsByKey(string key)
        {
            return ClientStatistics.FromByteArray(await cache.GetCachedValueAsync(key));
        }

        private async Task UpdateClientStatisticsAsync(ClientStatistics? statistics, string key, int maxRequests)
        {
            if (statistics is not null)
            {
                if (statistics.Count >= maxRequests && statistics.Count > 0)
                {
                    statistics.Dequeue();
                }
                statistics.Enqueue(DateTime.UtcNow);

                await cache.SetCachedValueAsync(key, statistics.ToByteArray());
            }
            else
            {
                ClientStatistics clientStats = new ClientStatistics(maxRequests, DateTime.UtcNow);

                await cache.SetCachedValueAsync(key, clientStats.ToByteArray());
            }
        }
    }
}
