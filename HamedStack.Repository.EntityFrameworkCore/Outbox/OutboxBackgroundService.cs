using System.Text.Json;
using System.Text.RegularExpressions;
using HamedStack.TheAggregateRoot.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HamedStack.TheRepository.EntityFrameworkCore.Outbox;

/// <summary>
/// A background service that processes unprocessed messages from the outbox table.
/// </summary>
public class OutboxBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxBackgroundService> _logger;
    private readonly IOptionsMonitor<OutboxBackgroundServiceOptions> _optionsMonitor;

    /// <summary>
    /// Initializes a new instance of the <see cref="OutboxBackgroundService"/> class.
    /// </summary>
    /// <param name="scopeFactory">A factory to create service scopes.</param>
    /// <param name="logger">A logger for logging messages and errors.</param>
    /// <param name="optionsMonitor">Monitors configuration options for the background service.</param>
    public OutboxBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxBackgroundService> logger,
        IOptionsMonitor<OutboxBackgroundServiceOptions> optionsMonitor)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _optionsMonitor = optionsMonitor;
    }

    /// <summary>
    /// Executes the background processing logic to handle unprocessed outbox messages.
    /// </summary>
    /// <param name="stoppingToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the background execution.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DbContextBase>();
            var dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();

            try
            {
                var options = _optionsMonitor.CurrentValue;
                var batchSize = options.BatchSize;

                // Fetch unprocessed messages from the outbox table.
                var messages = await dbContext.OutboxMessages
                    .Where(m => !m.IsProcessed)
                    .OrderBy(m => m.CreatedOn)
                    .Take(batchSize)
                    .ToListAsync(stoppingToken);

                if (messages.Count == 0)
                {
                    _logger.LogInformation("No unprocessed messages found. Waiting before next poll.");
                    await Task.Delay(TimeSpan.FromSeconds(options.PollingIntervalSeconds * 2), stoppingToken);
                    continue;
                }

                foreach (var message in messages)
                {
                    try
                    {
                        var eventKey = ExtractEventKey(message.Content);
                        var eventType = ResolveEventType(eventKey);

                        if (eventType == null)
                        {
                            _logger.LogWarning("Unknown event type for key {EventKey}", eventKey);
                            continue;
                        }

                        var domainEvent = JsonSerializer.Deserialize(message.Content, eventType)!;
                        await dispatcher.DispatchEventAsync(domainEvent, stoppingToken);

                        message.IsProcessed = true;
                        message.ProcessedOn = DateTime.Now;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to process message {MessageId}", message.Id);
                        message.IsProcessed = false;
                        message.RetryCount = (message.RetryCount ?? 0) + 1;
                        message.ProcessedOn = DateTime.Now;
                    }
                }

                await dbContext.SaveChangesAsync(stoppingToken);

                // Wait for the configured polling interval before processing the next batch.
                await Task.Delay(TimeSpan.FromSeconds(options.PollingIntervalSeconds), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical failure in OutboxBackgroundService");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

    /// <summary>
    /// Extracts the event key from the serialized message content.
    /// </summary>
    /// <param name="content">The serialized message content.</param>
    /// <returns>The event key extracted from the content.</returns>
    private static string ExtractEventKey(string content) =>
        new Regex("\"EventKey\":\"(.+?)\"").Match(content).Groups[1].Value;

    /// <summary>
    /// Resolves the event type based on the event key.
    /// </summary>
    /// <param name="eventKey">The event key representing the type of the event.</param>
    /// <returns>The resolved <see cref="Type"/> of the event, or null if not found.</returns>
    private static Type? ResolveEventType(string eventKey) =>
        AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => t.AssemblyQualifiedName == eventKey);
}