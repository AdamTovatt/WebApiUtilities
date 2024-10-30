using Microsoft.Extensions.DependencyInjection;
using System;

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
    }
}
