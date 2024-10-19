// ReSharper disable UnusedMember.Global

namespace HamedStack.TheRepository;

/// <summary>
/// Generic repository interface for handling domain models and their corresponding database models.
/// Inherits from <see cref="IReadRepository{TDomainModel, TDatabaseModel}"/> to provide read operations.
/// </summary>
/// <typeparam name="TDomainModel">The domain model type.</typeparam>
/// <typeparam name="TDatabaseModel">The database model type, constrained to be a class.</typeparam>
public interface IRepository<TDomainModel, TDatabaseModel> : IReadRepository<TDomainModel, TDatabaseModel>
    where TDatabaseModel : class
{
    /// <summary>
    /// Gets the <see cref="IUnitOfWork"/> instance associated with this repository.
    /// </summary>
    IUnitOfWork UnitOfWork { get; }

    /// <summary>
    /// Adds a domain model entity asynchronously.
    /// </summary>
    /// <param name="entity">The domain model entity to add.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The added domain model entity.</returns>
    Task<TDomainModel> AddAsync(TDomainModel entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple domain model entities asynchronously.
    /// </summary>
    /// <param name="entities">A collection of domain model entities to add.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A collection of the added domain model entities.</returns>
    Task<IEnumerable<TDomainModel>> AddRangeAsync(IEnumerable<TDomainModel> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a domain model entity asynchronously.
    /// </summary>
    /// <param name="entity">The domain model entity to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task DeleteAsync(TDomainModel entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a domain model entity by its identifier asynchronously.
    /// </summary>
    /// <typeparam name="TKey">The type of the entity identifier, constrained to be non-null.</typeparam>
    /// <param name="id">The identifier of the entity to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task DeleteAsync<TKey>(TKey id, CancellationToken cancellationToken = default) where TKey : notnull;

    /// <summary>
    /// Deletes a domain model entity by composite keys asynchronously.
    /// </summary>
    /// <param name="ids">An array of identifiers representing the composite keys of the entity to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task DeleteAsync(object[] ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes multiple domain model entities asynchronously.
    /// </summary>
    /// <param name="entities">A collection of domain model entities to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task DeleteRangeAsync(IEnumerable<TDomainModel> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a domain model entity asynchronously.
    /// </summary>
    /// <param name="entity">The domain model entity to update.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task UpdateAsync(TDomainModel entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates multiple domain model entities asynchronously.
    /// </summary>
    /// <param name="entities">A collection of domain model entities to update.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task UpdateRangeAsync(IEnumerable<TDomainModel> entities, CancellationToken cancellationToken = default);
}

