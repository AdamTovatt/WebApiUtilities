using System;

namespace Sakur.WebApiUtilities.RateLimiting
{
    /// <summary>
    /// Used to limit the amount of requests a client can make to a specific endpoint
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class Limit : Attribute
    {
        /// <summary>
        /// The time window in seconds during which MaxRequests amount of requests are allowed
        /// </summary>
        public int TimeWindow { get; set; }

        /// <summary>
        /// The amount of requests allowed during the time specified in seconds in TimeWindow
        /// </summary>
        public int MaxRequests { get; set; }
    }
}
