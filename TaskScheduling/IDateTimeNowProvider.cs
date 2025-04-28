using System;

namespace WebApiUtilities.TaskScheduling
{
    /// <summary>
    /// Represents a provider that can provide the a DateTime instance representing now. This is so that time of day tasks can be configured to run using local time, or utc, or any other way of getting the now time.
    /// </summary>
    public interface IDateTimeNowProvider
    {
        /// <summary>
        /// Gets the current time.
        /// </summary>
        /// <returns>An instance of <see cref="DateTime"/> representing "now".</returns>
        DateTime GetNow();
    }
}
