using HamedStack.TheAggregateRoot.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HamedStack.TheRepository.EntityFrameworkCore.Interceptors;

/// <summary>
/// A save changes interceptor that automatically sets audit properties (such as creation and modification timestamps)
/// for entities that implement the <see cref="IAudit"/> interface.
/// </summary>
public sealed class AuditInterceptor : SaveChangesInterceptor
{
    /// <summary>
    /// Intercepts the save changes operation and sets audit properties (e.g., <see cref="IAudit.CreatedOn"/> and 
    /// <see cref="IAudit.ModifiedOn"/>) for entities that implement the <see cref="IAudit"/> interface.
    /// </summary>
    /// <param name="eventData">The <see cref="SaveChangesCompletedEventData"/> containing information about the save operation.</param>
    /// <param name="result">The number of state entries written to the database.</param>
    /// <returns>The result of the save operation after audit properties are updated.</returns>
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

    /// <summary>
    /// Asynchronously intercepts the save changes operation and sets audit properties (e.g., <see cref="IAudit.CreatedOn"/> and
    /// <see cref="IAudit.ModifiedOn"/>) for entities that implement the <see cref="IAudit"/> interface.
    /// </summary>
    /// <param name="eventData">The <see cref="SaveChangesCompletedEventData"/> containing information about the save operation.</param>
    /// <param name="result">The number of state entries written to the database.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous save operation, after audit properties are updated.</returns>
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
