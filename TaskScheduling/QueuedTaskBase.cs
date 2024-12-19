using System.Threading;
using System.Threading.Tasks;

namespace Sakur.WebApiUtilities.TaskScheduling
{
    /// <summary>
    /// Base class for queued tasks. Queued tasks are tasks that are queued to run straight away.
    /// They run as soon as they can but in the background.
    /// It's like a safer way to create a normal .NET Task and not await it, because if you do that it might just never finish because it disappears mid way through.
    /// </summary>
    public abstract class QueuedTaskBase
    {
        /// <summary>
        /// Executes the queued task.
        /// </summary>
        public abstract Task ExecuteAsync(CancellationToken cancellationToken);
    }
}