using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using WebApiUtilities.TaskScheduling;

namespace Sakur.WebApiUtilities.TaskScheduling
{
    /// <summary>
    /// Extension methods for adding scheduled tasks to the DI container.
    /// </summary>
    public static class ScheduledTaskServiceCollectionExtensions
    {
        /// <summary>
        /// Extension method for adding scheduled tasks to the DI container.
        /// </summary>
        public static IServiceCollection AddScheduledTasks(this IServiceCollection services, params Type[] taskTypes)
        {
            return AddScheduledTasksInternal(services, new UtcNowProvider(), taskTypes);
        }

        /// <summary>
        /// Extension method for adding scheduled tasks to the DI container.
        /// </summary>
        public static IServiceCollection AddScheduledTasks(this IServiceCollection services, IDateTimeNowProvider dateTimeNowProvider, params Type[] taskTypes)
        {
            return AddScheduledTasksInternal(services, dateTimeNowProvider, taskTypes);
        }

        private static IServiceCollection AddScheduledTasksInternal(IServiceCollection services, IDateTimeNowProvider dateTimeNowProvider, params Type[] taskTypes)
        {
            foreach (Type taskType in taskTypes)
            {
                if (!typeof(ScheduledTaskBase).IsAssignableFrom(taskType))
                {
                    throw new ArgumentException($"Type {taskType.Name} does not inherit from {nameof(ScheduledTaskBase)}");
                }

                services.AddSingleton(typeof(ScheduledTaskBase), taskType);
            }

            services.AddSingleton(dateTimeNowProvider);

            services.AddSingleton(provider =>
            {
                IEnumerable<ScheduledTaskBase> tasks = provider.GetServices<ScheduledTaskBase>();
                ILogger<ScheduledTaskManager>? logger = provider.GetService<ILogger<ScheduledTaskManager>>();
                IDateTimeNowProvider dateTimeNowProviderFromDi = provider.GetRequiredService<IDateTimeNowProvider>();

                return new ScheduledTaskManager(tasks, logger, dateTimeNowProviderFromDi);
            });

            services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<ScheduledTaskManager>());

            return services;
        }


        /// <summary>
        /// Extension method for adding queued task processing to the DI container.
        /// </summary>
        public static IServiceCollection AddQueuedTaskProcessing(this IServiceCollection services)
        {
            services.AddSingleton(BackgroundTaskQueue.Instance);
            services.AddHostedService<QueuedTaskProcessor>();
            return services;
        }
    }
}
