using HamedStack.TheAggregateRoot.Events;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace HamedStack.TheRepository.EntityFrameworkCore.Interceptors;

public sealed class DomainEventInterceptor : SaveChangesInterceptor
{
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public DomainEventInterceptor(IDomainEventDispatcher domainEventDispatcher)
    {
        _domainEventDispatcher = domainEventDispatcher;
    }

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

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        var dbContext = eventData.Context;
        if (dbContext == null)
            return base.SavedChanges(eventData, result);

        var output = base.SavedChanges(eventData, result);
        if (dbContext.Database.CurrentTransaction?.GetDbTransaction().Connection == null) return output;


        var entitiesWithEvents = dbContext.ChangeTracker.Entries<IDomainEvent>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Count != 0)
            .ToList();

        foreach (var entity in entitiesWithEvents)
        {
            var events = entity.DomainEvents;
            entity.ClearDomainEvents();
            _domainEventDispatcher.DispatchEventsAsync(events).GetAwaiter().GetResult();
        }

        return output;
    }
}