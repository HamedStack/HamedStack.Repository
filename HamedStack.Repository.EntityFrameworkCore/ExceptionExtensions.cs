using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

// ReSharper disable UnusedMember.Global

namespace HamedStack.TheRepository.EntityFrameworkCore;

/// <summary>
/// Provides extension methods.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Determines whether the specified exception is a <see cref="DbUpdateConcurrencyException"/>.
    /// </summary>
    /// <param name="ex">The exception to check.</param>
    /// <returns><c>true</c> if the exception is a <see cref="DbUpdateConcurrencyException"/>; otherwise, <c>false</c>.</returns>
    public static bool IsDbUpdateConcurrencyException(this Exception ex)
    {
        return ex is DbUpdateConcurrencyException;
    }

    /// <summary>
    /// Handles concurrency exceptions by resolving conflicts based on the provided strategy.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="context">The DbContext instance.</param>
    /// <param name="ex">The DbUpdateConcurrencyException to handle.</param>
    /// <param name="resolveConflict">The function to resolve property conflicts. (property, proposedValue, databaseValue)</param>
    /// <exception cref="NotSupportedException">Thrown when the entity type is not supported.</exception>
    public static void ResolveConcurrency<TEntity>(
        this DbContext context,
        DbUpdateConcurrencyException ex,
        Func<PropertyEntry, object?, object?, object?> resolveConflict)
        where TEntity : class
    {
        foreach (var entry in ex.Entries)
        {
            if (entry.Entity is TEntity)
            {
                var proposedValues = entry.CurrentValues;
                var databaseValues = entry.GetDatabaseValues();

                if (databaseValues == null)
                {
                    throw new InvalidOperationException(
                        "The entity no longer exists in the database.");
                }

                foreach (var property in proposedValues.Properties)
                {
                    var propertyEntry = entry.Property(property.Name);
                    var proposedValue = proposedValues[property];
                    var databaseValue = databaseValues[property];

                    proposedValues[property] = resolveConflict(propertyEntry, proposedValue, databaseValue);
                }

                entry.OriginalValues.SetValues(databaseValues);
            }
            else
            {
                throw new NotSupportedException(
                    $"Don't know how to handle concurrency conflicts for {entry.Metadata.Name}");
            }
        }
    }

}
