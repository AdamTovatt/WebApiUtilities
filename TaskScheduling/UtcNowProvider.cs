using System;

namespace WebApiUtilities.TaskScheduling
{
    /// <summary>
    /// Provides "now" in utc.
    /// </summary>
    public class UtcNowProvider : IDateTimeNowProvider
    {
        /// <summary>
        /// Returns "now" in utc.
        /// </summary>
        /// <returns>The current date and time, in utc.</returns>
        public DateTime GetNow()
        {
            return DateTime.UtcNow;
        }
    }
}
