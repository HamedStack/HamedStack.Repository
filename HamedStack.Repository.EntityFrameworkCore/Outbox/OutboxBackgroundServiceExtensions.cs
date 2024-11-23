using Microsoft.Extensions.DependencyInjection;

namespace HamedStack.TheRepository.EntityFrameworkCore.Outbox;

/// <summary>
/// Extension methods for adding the <see cref="OutboxBackgroundService"/> to the service collection.
/// </summary>
public static class OutboxBackgroundServiceExtensions
{
    /// <summary>
    /// Adds the <see cref="OutboxBackgroundService"/> to the service collection with the specified options.
    /// </summary>
    /// <param name="services">The service collection to which the background service is added.</param>
    /// <param name="configureOptions">An action to configure the options for the service.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddOutboxBackgroundService(this IServiceCollection services, Action<OutboxBackgroundServiceOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddHostedService<OutboxBackgroundService>();
        return services;
    }

    /// <summary>
    /// Adds the <see cref="OutboxBackgroundService"/> to the service collection with default options.
    /// </summary>
    /// <param name="services">The service collection to which the background service is added.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddOutboxBackgroundService(this IServiceCollection services)
    {
        services.Configure<OutboxBackgroundServiceOptions>(options =>
        {
            options.PollingIntervalSeconds = 10;
            options.BatchSize = 100;            
        });

        services.AddHostedService<OutboxBackgroundService>();
        return services;
    }
}