using System;

namespace Sakur.WebApiUtilities.TaskScheduling
{
    /// <summary>
    /// A task that should be run at regular intervals.
    /// </summary>
    public abstract class IntervalTask : ScheduledTaskBase
    {
        /// <summary>
        /// Gets how frequently the task should run (e.g., every minute).
        /// </summary>
        public abstract TimeSpan Interval { get; }

        /// <summary>
        /// Gets how long to wait before the task is first executed.
        /// </summary>
        public abstract TimeSpan InitialStartDelay { get; }
    }
}
