using HamedStack.TheAggregateRoot.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HamedStack.TheRepository.EntityFrameworkCore.Interceptors;

/// <summary>
/// A save changes interceptor that implements soft delete functionality for entities that implement the <see cref="ISoftDelete"/> interface.
/// </summary>
public class SoftDeleteInterceptor : SaveChangesInterceptor
{
    /// <summary>
    /// Intercepts the save changes operation and applies soft delete logic to entities marked for deletion.
    /// </summary>
    /// <param name="eventData">The <see cref="SaveChangesCompletedEventData"/> containing information about the save operation.</param>
    /// <param name="result">The number of state entries written to the database.</param>
    /// <returns>The result of the base <see cref="SavedChanges"/> method, after applying soft delete logic.</returns>
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

    /// <summary>
    /// Asynchronously intercepts the save changes operation and applies soft delete logic to entities marked for deletion.
    /// </summary>
    /// <param name="eventData">The <see cref="SaveChangesCompletedEventData"/> containing information about the save operation.</param>
    /// <param name="result">The number of state entries written to the database.</param>
    /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous save operation, with soft delete logic applied.</returns>
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
