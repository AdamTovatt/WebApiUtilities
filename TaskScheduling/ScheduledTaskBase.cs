using System.Threading;
using System.Threading.Tasks;

namespace Sakur.WebApiUtilities.TaskScheduling
{
    /// <summary>
    /// Base class for scheduled tasks. Scheduled tasks are tasks that run automatically at regular intervals or at specific times.
    /// </summary>
    public abstract class ScheduledTaskBase
    {
        /// <summary>
        /// Protected constructor.
        /// </summary>
        protected ScheduledTaskBase() { }

        /// <summary>
        /// Will execute the task.
        /// </summary>
        /// <param name="cancellationToken">Used to cancel the task.</param>
        /// <returns></returns>
        public abstract Task ExecuteAsync(CancellationToken cancellationToken);
    }
}
