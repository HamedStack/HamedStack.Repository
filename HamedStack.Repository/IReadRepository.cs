using System.Linq.Expressions;
using HamedStack.Specification;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global

namespace HamedStack.TheRepository;

/// <summary>
/// Defines a read-only repository interface that supports querying and retrieving entities from the underlying data source.
/// </summary>
/// <typeparam name="TEntity">The type of the entity managed by this repository.</typeparam>
public interface IReadRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Gets the queryable collection of entities, which can be used to construct LINQ queries.
    /// </summary>
    IQueryable<TEntity> Query { get; }

    /// <summary>
    /// Determines whether all entities satisfy the specified specification asynchronously.
    /// </summary>
    /// <param name="specification">The specification to test the entities against.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a boolean indicating whether all entities match the specification.</returns>
    Task<bool> AllAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether all entities satisfy the specified predicate asynchronously.
    /// </summary>
    /// <param name="predicate">The predicate to test the entities against.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a boolean indicating whether all entities match the predicate.</returns>
    Task<bool> AllAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether any entity satisfies the specified specification asynchronously.
    /// </summary>
    /// <param name="specification">The specification to test the entities against.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a boolean indicating whether any entity matches the specification.</returns>
    Task<bool> AnyAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether any entity satisfies the specified predicate asynchronously.
    /// </summary>
    /// <param name="predicate">The predicate to test the entities against.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a boolean indicating whether any entity matches the predicate.</returns>
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of entities that match the specified specification asynchronously.
    /// </summary>
    /// <param name="specification">The specification to filter the entities.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the count of entities that match the specification.</returns>
    Task<int> CountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total count of entities in the repository asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the total count of entities.</returns>
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of entities that match the specified predicate asynchronously.
    /// </summary>
    /// <param name="predicate">The predicate to filter the entities.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the count of entities that match the predicate.</returns>
    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the first entity that matches the specified specification asynchronously or returns null if no entity is found.
    /// </summary>
    /// <param name="specification">The specification to filter the entities.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the first matching entity or null.</returns>
    Task<TEntity?> FirstOrDefaultAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the first entity that matches the specified predicate asynchronously or returns null if no entity is found.
    /// </summary>
    /// <param name="predicate">The predicate to filter the entities.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the first matching entity or null.</returns>
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a collection of all entities.</returns>
    Task<List<TEntity>> GetAll(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities that match the specified specification asynchronously.
    /// </summary>
    /// <param name="specification">The specification to filter the entities.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a collection of matching entities.</returns>
    Task<List<TEntity>> GetAll(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities that match the specified query asynchronously.
    /// </summary>
    /// <param name="query">The query to filter the entities.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a collection of matching entities.</returns>
    Task<List<TEntity>> GetAll(IQueryable<TEntity> query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an asynchronous enumerable of all entities.
    /// </summary>
    /// <returns>An asynchronous enumerable of all entities.</returns>
    IAsyncEnumerable<TEntity> GetAsyncEnumerable();

    /// <summary>
    /// Gets an entity by its identifier asynchronously.
    /// </summary>
    /// <typeparam name="TKey">The type of the identifier.</typeparam>
    /// <param name="id">The identifier of the entity to find.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the matching entity or null.</returns>
    ValueTask<TEntity?> GetByIdAsync<TKey>(TKey id, CancellationToken cancellationToken = default) where TKey : notnull;

    /// <summary>
    /// Gets an entity by its composite identifiers asynchronously.
    /// </summary>
    /// <param name="ids">An array of objects representing the composite identifiers of the entity.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A value task representing the asynchronous operation. The task result contains the matching entity or null.</returns>
    ValueTask<TEntity?> GetByIdsAsync(object[] ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a paginated collection of entities asynchronously.
    /// </summary>
    /// <param name="pageNumber">The page number to retrieve.</param>
    /// <param name="pageSize">The size of the page to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a collection of entities for the specified page.</returns>
    Task<List<TEntity>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a paginated collection of entities that match the specified specification asynchronously.
    /// </summary>
    /// <param name="specification">The specification to filter the entities.</param>
    /// <param name="pageNumber">The page number to retrieve.</param>
    /// <param name="pageSize">The size of the page to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a collection of entities for the specified page.</returns>
    Task<List<TEntity>> GetPagedAsync(ISpecification<TEntity> specification, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a paginated collection of entities that match the specified query asynchronously.
    /// </summary>
    /// <param name="query">The query to filter the entities.</param>
    /// <param name="pageNumber">The page number to retrieve.</param>
    /// <param name="pageSize">The size of the page to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a collection of entities for the specified page.</returns>
    Task<List<TEntity>> GetPagedAsync(IQueryable<TEntity> query, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the single entity that matches the specified specification asynchronously or returns null if no entity is found.
    /// </summary>
    /// <param name="specification">The specification to filter the entities.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the single matching entity or null.</returns>
    Task<TEntity?> SingleOrDefaultAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the single entity that matches the specified predicate asynchronously or returns null if no entity is found.
    /// </summary>
    /// <param name="predicate">The predicate to filter the entities.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the single matching entity or null.</returns>
    Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
}
