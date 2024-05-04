using HamedStack.TheAggregateRoot.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HamedStack.TheRepository.EntityFrameworkCore.Interceptors;

public sealed class AuditInterceptor : SaveChangesInterceptor
{
    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        var dbContext = eventData.Context;
        if (dbContext == null)
            return base.SavedChanges(eventData, result);

        var entries = dbContext.ChangeTracker.Entries<IAudit>();

        foreach (var entityEntry in entries)
        {
            switch (entityEntry.State)
            {
                case EntityState.Added:
                    entityEntry.Entity.CreatedOn = DateTimeOffset.UtcNow;
                    break;

                case EntityState.Modified:
                    entityEntry.Entity.ModifiedOn = DateTimeOffset.UtcNow;
                    break;
            }
        }

        return base.SavedChanges(eventData, result);
    }

    public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result,
        CancellationToken cancellationToken = new())
    {
        var dbContext = eventData.Context;
        if (dbContext == null)
            return base.SavedChangesAsync(eventData, result, cancellationToken);

        var entries = dbContext.ChangeTracker.Entries<IAudit>();

        foreach (var entityEntry in entries)
        {
            switch (entityEntry.State)
            {
                case EntityState.Added:
                    entityEntry.Entity.CreatedOn = DateTimeOffset.UtcNow;
                    break;
                case EntityState.Modified:
                    entityEntry.Entity.ModifiedOn = DateTimeOffset.UtcNow;
                    break;
            }
        }

        return base.SavedChangesAsync(eventData, result, cancellationToken);
    }
}