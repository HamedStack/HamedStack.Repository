using Ardalis.Specification;
using System.Linq.Expressions;
// ReSharper disable UnusedMember.Global

namespace HamedStack.TheRepository;

/// <summary>
/// Generic read-only repository interface for handling domain and database models.
/// Provides methods for querying data with various filters and specifications.
/// </summary>
/// <typeparam name="TDomainModel">The domain model type.</typeparam>
/// <typeparam name="TDatabaseModel">The database model type, constrained to be a class.</typeparam>
public interface IReadRepository<TDomainModel, TDatabaseModel> where TDatabaseModel : class
{
    /// <summary>
    /// Provides an <see cref="IQueryable{T}"/> for querying the database model entities.
    /// </summary>
    IQueryable<TDatabaseModel> Query { get; }

    /// <summary>
    /// Determines asynchronously whether any entities satisfy the given specification.
    /// </summary>
    /// <param name="specification">The specification used to filter entities.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if any entities match the specification; otherwise, false.</returns>
    Task<bool> AnyAsync(ISpecification<TDatabaseModel> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines asynchronously whether any entities satisfy the given predicate.
    /// </summary>
    /// <param name="predicate">The predicate expression to filter entities.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if any entities match the predicate; otherwise, false.</returns>
    Task<bool> AnyAsync(Expression<Func<TDatabaseModel, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts asynchronously the number of entities that satisfy the given specification.
    /// </summary>
    /// <param name="specification">The specification used to filter entities.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the number of entities that match the specification.</returns>
    Task<int> CountAsync(ISpecification<TDatabaseModel> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts asynchronously the total number of entities.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the total number of entities.</returns>
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts asynchronously the number of entities that satisfy the given predicate.
    /// </summary>
    /// <param name="predicate">The predicate expression to filter entities.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the number of entities that match the predicate.</returns>
    Task<int> CountAsync(Expression<Func<TDatabaseModel, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the first entity or a default value asynchronously that matches the given specification.
    /// </summary>
    /// <param name="specification">The specification used to filter entities.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the first matching domain model entity or null if no entity is found.</returns>
    Task<TDomainModel?> FirstOrDefaultAsync(ISpecification<TDatabaseModel> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the first entity or a default value asynchronously that matches the given predicate.
    /// </summary>
    /// <param name="predicate">The predicate expression to filter entities.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the first matching domain model entity or null if no entity is found.</returns>
    Task<TDomainModel?> FirstOrDefaultAsync(Expression<Func<TDatabaseModel, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all entities asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of all domain model entities.</returns>
    Task<List<TDomainModel>> GetAll(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all entities asynchronously that match the given specification.
    /// </summary>
    /// <param name="specification">The specification used to filter entities.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of domain model entities matching the specification.</returns>
    Task<List<TDomainModel>> GetAll(ISpecification<TDatabaseModel> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all entities asynchronously and projects them into the specified result type using the given specification.
    /// </summary>
    /// <typeparam name="TResult">The type of the projected result.</typeparam>
    /// <param name="specification">The specification used to filter and project entities.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of projected results.</returns>
    Task<List<TResult>> GetAll<TResult>(ISpecification<TDatabaseModel, TResult> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all entities asynchronously from a specified query.
    /// </summary>
    /// <param name="query">The query used to filter entities.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of domain model entities matching the query.</returns>
    Task<List<TDomainModel>> GetAll(IQueryable<TDatabaseModel> query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Provides an async stream of domain model entities.
    /// </summary>
    /// <returns>An asynchronous enumerable of domain model entities.</returns>
    IAsyncEnumerable<TDomainModel> GetAsyncEnumerable();

    /// <summary>
    /// Provides an async stream of domain model entities matching the given specification.
    /// </summary>
    /// <param name="specification">The specification used to filter entities.</param>
    /// <returns>An asynchronous enumerable of domain model entities matching the specification.</returns>
    IAsyncEnumerable<TDomainModel> GetAsyncEnumerable(ISpecification<TDatabaseModel> specification);

    /// <summary>
    /// Retrieves an entity asynchronously by its identifier.
    /// </summary>
    /// <typeparam name="TKey">The type of the entity identifier, constrained to be non-null.</typeparam>
    /// <param name="id">The identifier of the entity.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the domain model entity with the specified identifier or null if no entity is found.</returns>
    ValueTask<TDomainModel?> GetByIdAsync<TKey>(TKey id, CancellationToken cancellationToken = default) where TKey : notnull;

    /// <summary>
    /// Retrieves an entity asynchronously by its composite keys.
    /// </summary>
    /// <param name="ids">An array of identifiers representing the composite keys of the entity.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the domain model entity with the specified composite keys or null if no entity is found.</returns>
    ValueTask<TDomainModel?> GetByIdsAsync(object[] ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of entities asynchronously.
    /// </summary>
    /// <param name="pageNumber">The page number to retrieve.</param>
    /// <param name="pageSize">The number of entities per page.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a paginated list of domain model entities.</returns>
    Task<List<TDomainModel>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of entities asynchronously that match the given specification.
    /// </summary>
    /// <param name="specification">The specification used to filter entities.</param>
    /// <param name="pageNumber">The page number to retrieve.</param>
    /// <param name="pageSize">The number of entities per page.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a paginated list of domain model entities matching the specification.</returns>
    Task<List<TDomainModel>> GetPagedAsync(ISpecification<TDatabaseModel> specification, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of entities asynchronously from a specified query.
    /// </summary>
    /// <param name="query">The query used to filter entities.</param>
    /// <param name="pageNumber">The page number to retrieve.</param>
    /// <param name="pageSize">The number of entities per page.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a paginated list of domain model entities matching the query.</returns>
    Task<List<TDomainModel>> GetPagedAsync(IQueryable<TDatabaseModel> query, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single entity or a default value asynchronously that matches the given specification.
    /// </summary>
    /// <param name="specification">The specification used to filter entities.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the matching domain model entity or null if no entity is found.</returns>
    Task<TDomainModel?> SingleOrDefaultAsync(ISpecification<TDatabaseModel> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single entity or a default value asynchronously that matches the given predicate.
    /// </summary>
    /// <param name="predicate">The predicate expression to filter entities.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the matching domain model entity or null if no entity is found.</returns>
    Task<TDomainModel?> SingleOrDefaultAsync(Expression<Func<TDatabaseModel, bool>> predicate, CancellationToken cancellationToken = default);
}



