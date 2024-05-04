using HamedStack.TheAggregateRoot.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HamedStack.TheRepository.EntityFrameworkCore.Interceptors;

public class SoftDeleteInterceptor : SaveChangesInterceptor
{
    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        var dbContext = eventData.Context;
        if (dbContext == null)
            return base.SavedChanges(eventData, result);

        var entries = dbContext.ChangeTracker.Entries<ISoftDelete>();

        foreach (var entityEntry in entries)
        {
            if (entityEntry.State == EntityState.Deleted)
            {
                entityEntry.State = EntityState.Modified;
                entityEntry.Entity.IsDeleted = true;
                entityEntry.Entity.DeletedOn = DateTimeOffset.Now;
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

        var entries = dbContext.ChangeTracker.Entries<ISoftDelete>();

        foreach (var entityEntry in entries)
        {
            if (entityEntry.State != EntityState.Deleted) continue;

            entityEntry.State = EntityState.Modified;
            entityEntry.Entity.IsDeleted = true;
            entityEntry.Entity.DeletedOn = DateTimeOffset.Now;
        }

        return base.SavedChangesAsync(eventData, result, cancellationToken);
    }
}