using System.Text.Json;
using HamedStack.TheAggregateRoot.Events;
using HamedStack.TheRepository.EntityFrameworkCore.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HamedStack.TheRepository.EntityFrameworkCore.Interceptors;

/// <summary>
/// A save changes interceptor that extracts domain events from entities and saves them as outbox messages 
/// for eventual processing.
/// </summary>
public class DomainEventOutboxInterceptor : SaveChangesInterceptor
{
    /// <summary>
    /// Asynchronously intercepts the saving changes operation and inserts outbox messages for domain events
    /// before the changes are saved to the database.
    /// </summary>
    /// <param name="eventData">The <see cref="DbContextEventData"/> containing information about the save operation.</param>
    /// <param name="result">The result of the save changes operation.</param>
    /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the interception result of the save operation.</returns>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = new())
    {
        if (eventData.Context != null)
        {
            InsertOutboxMessages(eventData.Context);
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Extracts domain events from tracked entities, converts them into outbox messages, and adds them to the outbox message set.
    /// </summary>
    /// <param name="dbContext">The <see cref="DbContext"/> where the changes are being tracked and saved.</param>
    private static void InsertOutboxMessages(DbContext dbContext)
    {
        var domainEvents = dbContext.ChangeTracker.Entries<IDomainEvent>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Count != 0)
            .SelectMany(e =>
            {
                var domainEvents = e.DomainEvents.ToList();
                e.ClearDomainEvents();
                return domainEvents;
            })
            .ToList();

        var outboxMessages = domainEvents.Select(domainEvent => new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Name = domainEvent.GetType().Name,
            Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
            CreatedOn = DateTime.Now,
            IsProcessed = false,
            ProcessedOn = null,
        }).ToList();

        if (outboxMessages.Count > 0)
            dbContext.Set<OutboxMessage>().AddRange(outboxMessages);
    }
}
