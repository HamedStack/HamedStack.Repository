using HamedStack.TheAggregateRoot.Events;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace HamedStack.TheRepository.EntityFrameworkCore.Interceptors;

/// <summary>
/// A save changes interceptor that dispatches domain events after changes are successfully saved to the database.
/// </summary>
public sealed class DomainEventInterceptor : SaveChangesInterceptor
{
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainEventInterceptor"/> class.
    /// </summary>
    /// <param name="domainEventDispatcher">The dispatcher responsible for dispatching domain events.</param>
    public DomainEventInterceptor(IDomainEventDispatcher domainEventDispatcher)
    {
        _domainEventDispatcher = domainEventDispatcher;
    }

    /// <summary>
    /// Asynchronously intercepts the save changes operation and dispatches domain events after the changes are saved.
    /// </summary>
    /// <param name="eventData">The <see cref="SaveChangesCompletedEventData"/> containing information about the save operation.</param>
    /// <param name="result">The number of state entries written to the database.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous save operation, dispatching domain events after completion.</returns>
    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result,
        CancellationToken cancellationToken = new())
    {
        var dbContext = eventData.Context;
        if (dbContext == null)
            return await base.SavedChangesAsync(eventData, result, cancellationToken);

        var output = await base.SavedChangesAsync(eventData, result, cancellationToken);

        if (dbContext.Database.CurrentTransaction?.GetDbTransaction().Connection == null) return output;

        var domainEvents = dbContext.ChangeTracker.Entries<IDomainEvent>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Count != 0)
            .SelectMany(e =>
            {
                var domainEvents = e.DomainEvents;
                e.ClearDomainEvents();
                return domainEvents;
            })
            .ToList();

        await _domainEventDispatcher.DispatchEventsAsync(domainEvents, cancellationToken);

        return output;
    }

    /// <summary>
    /// Intercepts the save changes operation and dispatches domain events after the changes are saved.
    /// </summary>
    /// <param name="eventData">The <see cref="SaveChangesCompletedEventData"/> containing information about the save operation.</param>
    /// <param name="result">The number of state entries written to the database.</param>
    /// <returns>The result of the save operation, after dispatching domain events.</returns>
    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        var dbContext = eventData.Context;
        if (dbContext == null)
            return base.SavedChanges(eventData, result);

        var output = base.SavedChanges(eventData, result);

        // Only proceed if there is an active transaction
        if (dbContext.Database.CurrentTransaction?.GetDbTransaction().Connection == null) return output;

        // Collect and clear domain events from entities
        var entitiesWithEvents = dbContext.ChangeTracker.Entries<IDomainEvent>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Count != 0)
            .ToList();

        foreach (var entity in entitiesWithEvents)
        {
            var events = entity.DomainEvents;
            entity.ClearDomainEvents();
            // Synchronously dispatch domain events
            _domainEventDispatcher.DispatchEventsAsync(events).GetAwaiter().GetResult();
        }

        return output;
    }
}
