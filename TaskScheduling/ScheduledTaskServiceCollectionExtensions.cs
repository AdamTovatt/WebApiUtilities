using Microsoft.Extensions.DependencyInjection;
using System;
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
            foreach (Type taskType in taskTypes)
            {
                if (!typeof(ScheduledTaskBase).IsAssignableFrom(taskType))
                {
                    throw new ArgumentException($"Type {taskType.Name} does not inherit from ScheduledTaskBase");
                }

                services.AddSingleton(typeof(ScheduledTaskBase), taskType);
            }

            services.AddHostedService<ScheduledTaskManager>();
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
