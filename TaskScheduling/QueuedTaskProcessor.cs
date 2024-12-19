using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sakur.WebApiUtilities.TaskScheduling;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebApiUtilities.TaskScheduling
{
    /// <summary>
    /// The background service that will actually process queued tasks.
    /// </summary>
    public class QueuedTaskProcessor : BackgroundService
    {
        private readonly BackgroundTaskQueue _taskQueue;
        private readonly ILogger<QueuedTaskProcessor> _logger;

        /// <summary>
        /// Creates an instance of the queued task processor.
        /// </summary>
        /// <param name="taskQueue"></param>
        /// <param name="logger"></param>
        public QueuedTaskProcessor(BackgroundTaskQueue taskQueue, ILogger<QueuedTaskProcessor> logger)
        {
            _taskQueue = taskQueue;
            _logger = logger;
        }

        /// <summary>
        /// The execute method for this queued task processor. Since it is a service it needs this, this is what starts it.
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                QueuedTaskBase task = await _taskQueue.DequeueTaskAsync(stoppingToken);
                try
                {
                    await task.ExecuteAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing a queued task.");
                }
            }
        }
    }
}
