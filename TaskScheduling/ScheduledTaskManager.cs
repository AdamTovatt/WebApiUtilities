using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApiUtilities.TaskScheduling;

namespace Sakur.WebApiUtilities.TaskScheduling
{
    /// <summary>
    /// Manages scheduled tasks.
    /// </summary>
    public class ScheduledTaskManager : IHostedService, IDisposable
    {
        private readonly List<ScheduledTaskBase> _tasks;
        private readonly List<Timer> _timers;
        private readonly ILogger<ScheduledTaskManager>? _logger;
        private readonly IDateTimeNowProvider _dateTimeNowProvider;

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="tasks">The tasks that should be scheduled.</param>
        /// <param name="logger">A logger that can log errors.</param>
        /// <param name="dateTimeNowProvider">A provider for getting the current time. Used so that what is considered to be "now" can be controlled so that tasks can easily be scheduled at a certain time of day in any time zone or similar.</param>
        public ScheduledTaskManager(IEnumerable<ScheduledTaskBase> tasks, ILogger<ScheduledTaskManager>? logger, IDateTimeNowProvider dateTimeNowProvider)
        {
            _tasks = tasks.ToList();
            _timers = new List<Timer>();
            _logger = logger;
            _dateTimeNowProvider = dateTimeNowProvider;
        }

        /// <summary>
        /// Will start the scheduled tasks.
        /// </summary>
        /// <param name="cancellationToken">Used to cancel the tasks.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Will be thrown if a scheduled task has been added but is of an unknown type.</exception>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (ScheduledTaskBase task in _tasks)
            {
                if (task is IntervalTask intervalTask) // Fixed interval scheduling
                {
                    Timer timer = new Timer(async _ => { await ExecuteTaskAsync(task, cancellationToken); }, null, intervalTask.InitialStartDelay, intervalTask.Interval);

                    _timers.Add(timer);
                }
                else if (task is TimeOfDayTask timeOfDayTask) // Specific time of day scheduling
                {
                    ScheduleDailyTask(timeOfDayTask, cancellationToken);
                }
                else
                {
                    throw new InvalidOperationException($"Unknown task type: {task.GetType().Name}");
                }
            }

            return Task.CompletedTask;
        }

        private void ScheduleDailyTask(TimeOfDayTask task, CancellationToken cancellationToken)
        {
            TimeSpan scheduledTime = task.ScheduledTime;
            DateTime now = _dateTimeNowProvider.GetNow();
            DateTime firstRun = new DateTime(now.Year, now.Month, now.Day, scheduledTime.Hours, scheduledTime.Minutes, 0);

            if (now > firstRun) firstRun = firstRun.AddDays(1); // Calculate the initial delay
            TimeSpan initialDelay = firstRun - now;

            // Set up the timer to execute at the specific time of day, repeating every 24 hours
            Timer timer = new Timer(async _ => { await ExecuteTaskAsync(task, cancellationToken); }, null, initialDelay, TimeSpan.FromDays(1));

            _timers.Add(timer);
        }

        private async Task ExecuteTaskAsync(ScheduledTaskBase task, CancellationToken cancellationToken)
        {
            try
            {
                await task.ExecuteAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                if (_logger != null)
                {
                    _logger.LogError(exception, $"An error occurred executing a scheduled task at {DateTime.Now}");
                }
            }
        }

        /// <summary>
        /// Will stop the scheduled tasks.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (Timer timer in _timers)
                timer.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Will dispose the timers.
        /// </summary>
        public void Dispose()
        {
            foreach (Timer timer in _timers)
                timer.Dispose();
        }
    }
}
