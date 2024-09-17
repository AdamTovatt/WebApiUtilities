using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Sakur.WebApiUtilities.RateLimiting
{
    /// <summary>
    /// Statistics for a client so they can be limited
    /// </summary>
    public class ClientStatistics : Queue<DateTime>
    {
        /// <summary>
        /// Empty constructor
        /// </summary>
        public ClientStatistics() : base() { }

        /// <summary>
        /// Constructor taking the maximum number of requests
        /// </summary>
        /// <param name="maxRequests"></param>
        public ClientStatistics(int maxRequests) : base(maxRequests) { }

        /// <summary>
        /// Constructor taking the maximum number of requests and the initial value
        /// </summary>
        /// <param name="maxRequests"></param>
        /// <param name="initialValue"></param>
        public ClientStatistics(int maxRequests, DateTime initialValue) : base(maxRequests)
        {
            Enqueue(initialValue);
        }

        /// <summary>
        /// Constructor taking an IEnumerable of values
        /// </summary>
        /// <param name="values"></param>
        public ClientStatistics(IEnumerable<DateTime> values) : base(values) { }

        /// <summary>
        /// Will return the statistics as a byte array
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteArray()
        {
            return Encoding.Default.GetBytes(JsonSerializer.Serialize(new List<DateTime>(this)));
        }

        /// <summary>
        /// Will return the statistics as a byte array
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static ClientStatistics FromByteArray(byte[]? bytes)
        {
            if (bytes == null)
                return new ClientStatistics();

            IEnumerable<DateTime>? requests = JsonSerializer.Deserialize<List<DateTime>>(Encoding.Default.GetString(bytes));

            if (requests == null)
                return new ClientStatistics();

            return new ClientStatistics(requests);
        }
    }
}
