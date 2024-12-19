using Sakur.WebApiUtilities.TaskScheduling;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace WebApiUtilities.TaskScheduling
{
    /// <summary>
    /// The background task queue that can queue and dequeue queued tasks.
    /// </summary>
    public class BackgroundTaskQueue
    {
        private readonly ConcurrentQueue<QueuedTaskBase> _workItems = new();
        private readonly SemaphoreSlim _signal = new(0);

        // Static instance for global access
        private static readonly BackgroundTaskQueue _instance = new BackgroundTaskQueue();

        /// <summary>
        /// Static property to access the singleton instance.
        /// </summary>
        public static BackgroundTaskQueue Instance => _instance;

        // Private constructor to prevent external instantiation
        private BackgroundTaskQueue() { }

        /// <summary>
        /// Will queue a queued task to run as soon as possible in the background.
        /// </summary>
        /// <param name="task">The task to queue for background execution.</param>
        public void QueueTask(QueuedTaskBase task)
        {
            _workItems.Enqueue(task);
            _signal.Release();
        }

        /// <summary>
        /// Will remove a queued task from the queue and return it.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to cancel the dequeue operation.</param>
        /// <returns>The dequeued task.</returns>
        public async Task<QueuedTaskBase> DequeueTaskAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                await _signal.WaitAsync(cancellationToken);

                if (_workItems.TryDequeue(out QueuedTaskBase? workItem) && workItem != null)
                {
                    return workItem;
                }
            }
        }
    }
}
