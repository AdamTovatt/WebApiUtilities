using System;

namespace Sakur.WebApiUtilities.TaskScheduling
{
    /// <summary>
    /// A task that is scheduled to run once a day.
    /// </summary>
    public abstract class TimeOfDayTask : ScheduledTaskBase
    {
        /// <summary>
        /// Gets the specific time of day when the task should run (e.g., 04:00).
        /// </summary>
        public abstract TimeSpan ScheduledTime { get; }
    }
}
