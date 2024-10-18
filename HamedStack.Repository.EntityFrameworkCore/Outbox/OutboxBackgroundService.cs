using System.Text.Json;
using System.Text.RegularExpressions;
using HamedStack.TheAggregateRoot.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HamedStack.TheRepository.EntityFrameworkCore.Outbox;

/// <summary>
/// A background service that processes outbox messages, dispatching domain events that are serialized in the message content.
/// This service runs periodically, checking for unprocessed outbox messages and dispatching the events.
/// </summary>
public class OutboxBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="OutboxBackgroundService"/> class.
    /// </summary>
    /// <param name="scopeFactory">The factory used to create service scopes for dependency injection.</param>
    public OutboxBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    /// <summary>
    /// Executes the background task, periodically processing unprocessed outbox messages and dispatching domain events.
    /// </summary>
    /// <param name="stoppingToken">A token that signals when the background task should stop.</param>
    /// <returns>A task that represents the background operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DbContextBase>();

                var messages = await dbContext.OutboxMessages
                    .Where(m => !m.IsProcessed)
                    .ToListAsync(stoppingToken);

                foreach (var message in messages)
                {
                    var eventKey = new Regex("\"EventKey\":\"(.+?)\"").Match(message.Content).Groups[1].ToString();

                    var eventType = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(x => x.GetTypes())
                        .FirstOrDefault(x => x.AssemblyQualifiedName == eventKey);

                    var domainEvent = JsonSerializer.Deserialize(message.Content, eventType!)!;
                    var dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();

                    try
                    {
                        await dispatcher.DispatchEventAsync(domainEvent, stoppingToken);
                        message.IsProcessed = true;
                        message.ProcessedOn = DateTimeOffset.Now;
                    }
                    catch (Exception)
                    {
                        message.IsProcessed = false;
                        message.ProcessedOn = DateTimeOffset.Now;
                        message.RetryCount = message.RetryCount == null ? 1 : message.RetryCount.Value + 1;
                    }
                }

                await dbContext.SaveChangesAsync(stoppingToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
