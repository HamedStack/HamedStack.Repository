using System.Linq.Expressions;
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using HamedStack.TheAggregateRoot.Abstractions;
using Microsoft.EntityFrameworkCore;

// ReSharper disable UseCollectionExpression
// ReSharper disable UnusedMember.Global

namespace HamedStack.TheRepository.EntityFrameworkCore;

/// <summary>
/// Represents a generic repository for performing CRUD operations on a specific entity type.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public class Repository<TEntity> : IRepository<TEntity>
    where TEntity : class
{
    private readonly DbContextBase _dbContext;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <param name="dbContext">The database context instance.</param>
    /// <param name="timeProvider">The time provider for auditing purposes.</param>
    public Repository(DbContextBase dbContext, TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    /// <summary>
    /// Gets the queryable collection of the specified entity type.
    /// </summary>
    public virtual IQueryable<TEntity> Query => DbSet.AsQueryable();

    /// <summary>
    /// Gets the unit of work instance associated with the repository.
    /// </summary>
    public virtual IUnitOfWork UnitOfWork => _dbContext;

    /// <summary>
    /// Gets the database context.
    /// </summary>
    protected virtual DbContextBase DbContext => _dbContext;

    /// <summary>
    /// Gets the DbSet of the specified entity type.
    /// </summary>
    protected virtual DbSet<TEntity> DbSet => _dbContext.Set<TEntity>();

    /// <summary>
    /// Adds an entity asynchronously.
    /// </summary>
    /// <param name="entity">The entity to be added.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The added entity.</returns>
    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        ApplyCreatedAuditTime(entity);
        await DbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    /// <summary>
    /// Adds a collection of entities asynchronously.
    /// </summary>
    /// <param name="entities">The collection of entities to be added.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of added entities.</returns>
    public virtual async Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        if (entities == null) throw new ArgumentNullException(nameof(entities));
        var result = new List<TEntity>();
        foreach (var entity in entities)
        {
            var output = await AddAsync(entity, cancellationToken);
            result.Add(output);
        }
        return result;
    }

    /// <summary>
    /// Checks if any entities satisfy a specified condition.
    /// </summary>
    /// <param name="specification">The specification defining the condition.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if any entity satisfies the condition, otherwise false.</returns>
    public virtual async Task<bool> AnyAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        if (specification == null) throw new ArgumentNullException(nameof(specification));
        return await ApplySpecification(specification, true).AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if any entities satisfy a specified predicate.
    /// </summary>
    /// <param name="predicate">The condition to evaluate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if any entity satisfies the condition, otherwise false.</returns>
    public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        return await DbSet.AnyAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Counts the number of entities satisfying a specified condition.
    /// </summary>
    /// <param name="specification">The specification defining the condition.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The count of entities satisfying the condition.</returns>
    public virtual async Task<int> CountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        if (specification == null) throw new ArgumentNullException(nameof(specification));
        return await ApplySpecification(specification, true).CountAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the total count of entities.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The total count of entities.</returns>
    public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(cancellationToken);
    }

    /// <summary>
    /// Counts the number of entities satisfying a specified predicate.
    /// </summary>
    /// <param name="predicate">The condition to evaluate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The count of entities satisfying the condition.</returns>
    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        return await DbSet.CountAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Deletes a specified entity.
    /// </summary>
    /// <param name="entity">The entity to be deleted.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public virtual async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        DbSet.Remove(entity);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Deletes an entity by a specified ID asynchronously.
    /// </summary>
    /// <typeparam name="TKey">The type of the ID.</typeparam>
    /// <param name="id">The ID of the entity to be deleted.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public virtual async Task DeleteAsync<TKey>(TKey id, CancellationToken cancellationToken = default) where TKey : notnull
    {
        if (id == null) throw new ArgumentNullException(nameof(id));
        if (!typeof(IIdentifier<TKey>).IsAssignableFrom(typeof(TEntity)))
        {
            throw new InvalidOperationException($"Entity type {typeof(TEntity).FullName} does not implement {typeof(IIdentifier<TKey>).FullName} interface.");
        }
        var entity = await DbSet.FindAsync(new object[] { id }, cancellationToken);
        if (entity != null) DbSet.Remove(entity);
    }

    /// <summary>
    /// Deletes an entity by its composite keys asynchronously.
    /// </summary>
    /// <param name="ids">The composite keys of the entity to be deleted.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public virtual async Task DeleteAsync(object[] ids, CancellationToken cancellationToken = default)
    {
        if (ids == null) throw new ArgumentNullException(nameof(ids));
        var entity = await DbSet.FindAsync(ids, cancellationToken);
        if (entity != null) DbSet.Remove(entity);
    }

    /// <summary>
    /// Deletes a collection of entities asynchronously.
    /// </summary>
    /// <param name="entities">The collection of entities to be deleted.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public virtual Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        if (entities == null) throw new ArgumentNullException(nameof(entities));
        DbSet.RemoveRange(entities);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Deletes a collection of entities satisfying a specified condition asynchronously.
    /// </summary>
    /// <param name="specification">The specification to identify the entities to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public virtual async Task DeleteRangeAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        if (specification == null) throw new ArgumentNullException(nameof(specification));
        var query = ApplySpecification(specification);
        await DeleteRangeAsync(query, cancellationToken);
    }

    /// <summary>
    /// Deletes a collection of entities from a given query asynchronously.
    /// </summary>
    /// <param name="query">The query to identify the entities to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public virtual async Task DeleteRangeAsync(IQueryable<TEntity> query, CancellationToken cancellationToken = default)
    {
        if (query == null) throw new ArgumentNullException(nameof(query));
        var entities = await GetAll(query, cancellationToken);
        await DeleteRangeAsync(entities, cancellationToken);
    }

    /// <summary>
    /// Gets the first entity matching a specified condition.
    /// </summary>
    /// <param name="specification">The specification defining the condition.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The first matching entity, or null if no match is found.</returns>
    public virtual async Task<TEntity?> FirstOrDefaultAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        if (specification == null) throw new ArgumentNullException(nameof(specification));
        return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the first entity matching a specified predicate.
    /// </summary>
    /// <param name="predicate">The predicate defining the condition.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The first matching entity, or null if no match is found.</returns>
    public virtual async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        return await DbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Gets all entities asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of all entities.</returns>
    public virtual async Task<List<TEntity>> GetAll(CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets all entities matching a specified condition asynchronously.
    /// </summary>
    /// <param name="specification">The specification defining the condition.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of all matching entities.</returns>
    public virtual async Task<List<TEntity>> GetAll(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        if (specification == null) throw new ArgumentNullException(nameof(specification));
        var queryResult = await ApplySpecification(specification).AsNoTracking().ToListAsync(cancellationToken);
        return specification.PostProcessingAction == null ? queryResult : specification.PostProcessingAction(queryResult).ToList();
    }

    /// <summary>
    /// Retrieves all entities that match the specified specification and projects them into a
    /// result type asynchronously.
    /// </summary>
    /// <typeparam name="TResult">The type of the result to project to.</typeparam>
    /// <param name="specification">The specification used to filter and project the entities.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a list of projected
    /// entities that match the criteria defined in the specification. If a post-processing action
    /// is defined in the specification, it will be applied to the result set before returning.
    /// </returns>
    public virtual async Task<List<TResult>> GetAll<TResult>(ISpecification<TEntity, TResult> specification, CancellationToken cancellationToken = default)
    {
        if (specification == null) throw new ArgumentNullException(nameof(specification));
        var queryResult = await ApplySpecification(specification).ToListAsync(cancellationToken);
        return specification.PostProcessingAction == null ? queryResult : specification.PostProcessingAction(queryResult).ToList();
    }

    /// <summary>
    /// Gets all entities from a given query asynchronously.
    /// </summary>
    /// <param name="query">The query to retrieve entities from.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of all entities in the query.</returns>
    public virtual async Task<List<TEntity>> GetAll(IQueryable<TEntity> query, CancellationToken cancellationToken = default)
    {
        if (query == null) throw new ArgumentNullException(nameof(query));
        return await query.AsNoTracking().ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the asynchronous enumerable of the entity type.
    /// </summary>
    /// <returns>An async enumerable of the entity type.</returns>
    public virtual IAsyncEnumerable<TEntity> GetAsyncEnumerable()
    {
        return DbSet.AsNoTracking().AsAsyncEnumerable();
    }

    /// <summary>
    /// Returns an asynchronous enumerable of entities that match the specified specification.
    /// </summary>
    /// <param name="specification">The specification used to filter the entities.</param>
    /// <returns>
    /// An <see cref="IAsyncEnumerable{TEntity}"/> representing the entities that satisfy the
    /// criteria defined in the specification.
    /// </returns>
    public virtual IAsyncEnumerable<TEntity> GetAsyncEnumerable(ISpecification<TEntity> specification)
    {
        if (specification == null) throw new ArgumentNullException(nameof(specification));
        return ApplySpecification(specification).AsNoTracking().AsAsyncEnumerable();
    }

    /// <summary>
    /// Gets an entity by its ID asynchronously.
    /// </summary>
    /// <typeparam name="TKey">The type of the ID.</typeparam>
    /// <param name="id">The ID of the entity to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The entity if found, or null if not found.</returns>
    public virtual async ValueTask<TEntity?> GetByIdAsync<TKey>(TKey id, CancellationToken cancellationToken = default) where TKey : notnull
    {
        if (id == null) throw new ArgumentNullException(nameof(id));
        if (!typeof(IIdentifier<TKey>).IsAssignableFrom(typeof(TEntity)))
        {
            throw new InvalidOperationException($"Entity type {typeof(TEntity).FullName} does not implement {typeof(IIdentifier<TKey>).FullName} interface.");
        }

        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <summary>
    /// Gets an entity by its composite keys asynchronously.
    /// </summary>
    /// <param name="ids">The composite keys of the entity to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The entity if found, or null if not found.</returns>
    public virtual async ValueTask<TEntity?> GetByIdsAsync(object[] ids, CancellationToken cancellationToken = default)
    {
        if (ids == null) throw new ArgumentNullException(nameof(ids));
        return await DbSet.FindAsync(ids, cancellationToken);
    }

    /// <summary>
    /// Gets a paginated list of entities asynchronously.
    /// </summary>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="pageSize">The size of each page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of entities in the specified page.</returns>
    public virtual async Task<List<TEntity>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        if (pageNumber <= 0) throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than zero.");
        if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");

        return await DbSet.Skip((pageNumber - 1) * pageSize).Take(pageSize).AsNoTracking().ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets a paginated list of entities matching a specified condition.
    /// </summary>
    /// <param name="specification">The specification defining the condition.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="pageSize">The size of each page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of entities in the specified page.</returns>
    public virtual async Task<List<TEntity>> GetPagedAsync(ISpecification<TEntity> specification, int pageNumber, int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber <= 0) throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than zero.");
        if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");

        var query = ApplySpecification(specification);
        return await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).AsNoTracking().ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets a paginated list of entities from a given query asynchronously.
    /// </summary>
    /// <param name="query">The query to retrieve entities from.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="pageSize">The size of each page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of entities in the specified page.</returns>
    public virtual async Task<List<TEntity>> GetPagedAsync(IQueryable<TEntity> query, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        if (pageNumber <= 0) throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than zero.");
        if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");

        return await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).AsNoTracking().ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets a single entity that matches a specified condition asynchronously.
    /// </summary>
    /// <param name="specification">The specification defining the condition.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The single matching entity, or null if no match is found.</returns>
    public virtual async Task<TEntity?> SingleOrDefaultAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        if (specification == null) throw new ArgumentNullException(nameof(specification));
        return await ApplySpecification(specification).SingleOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Gets a single entity that matches a specified predicate asynchronously.
    /// </summary>
    /// <param name="predicate">The condition to evaluate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The single matching entity, or null if no match is found.</returns>
    public virtual async Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        return await DbSet.SingleOrDefaultAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Updates a specified entity asynchronously.
    /// </summary>
    /// <param name="entity">The entity to be updated.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public virtual Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        ApplyModifiedAuditTime(entity);
        DbSet.Update(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Updates a collection of entities asynchronously.
    /// </summary>
    /// <param name="entities">The collection of entities to be updated.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public virtual async Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        if (entities == null) throw new ArgumentNullException(nameof(entities));
        foreach (var entity in entities)
        {
            await UpdateAsync(entity, cancellationToken);
        }
    }

    /// <summary>
    /// Applies the created audit time and user to the entity if it implements the <see cref="IAudit"/> interface.
    /// </summary>
    /// <param name="entity">The entity to which the created audit time will be applied.</param>
    protected virtual void ApplyCreatedAuditTime(TEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (entity is IAudit audit)
        {
            audit.CreatedOn = _timeProvider.GetUtcNow();
            audit.CreatedBy = ToString();
        }
    }

    /// <summary>
    /// Applies the modified audit time and user to the entity if it implements the <see cref="IAudit"/> interface.
    /// </summary>
    /// <param name="entity">The entity to which the modified audit time will be applied.</param>
    protected virtual void ApplyModifiedAuditTime(TEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (entity is IAudit audit)
        {
            audit.ModifiedOn = _timeProvider.GetUtcNow();
            audit.ModifiedBy = ToString();
        }
    }

    /// <summary>
    /// Applies the given specification to the entities, returning a queryable collection that can
    /// be further queried.
    /// </summary>
    /// <param name="specification">The specification used to filter and shape the query.</param>
    /// <param name="evaluateCriteriaOnly">
    /// Indicates whether to evaluate only the criteria portion of the specification without
    /// applying includes or projections.
    /// </param>
    /// <returns>
    /// An <see cref="IQueryable{TEntity}"/> representing the query that matches the criteria
    /// defined in the specification.
    /// </returns>
    protected virtual IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> specification, bool evaluateCriteriaOnly = false)
    {
        if (specification == null) throw new ArgumentNullException(nameof(specification));
        return SpecificationEvaluator.Default.GetQuery(DbSet.AsQueryable(), specification, evaluateCriteriaOnly);
    }

    /// <summary>
    /// Applies the given specification to the entities and projects them into a result type,
    /// returning a queryable collection that can be further queried.
    /// </summary>
    /// <typeparam name="TResult">The type of the result to project to.</typeparam>
    /// <param name="specification">
    /// The specification used to filter and shape the query, including the projection logic.
    /// </param>
    /// <returns>
    /// An <see cref="IQueryable{TResult}"/> representing the query that matches the criteria
    /// defined in the specification and projected to the specified result type.
    /// </returns>
    protected virtual IQueryable<TResult> ApplySpecification<TResult>(ISpecification<TEntity, TResult> specification)
    {
        if (specification == null) throw new ArgumentNullException(nameof(specification));
        return SpecificationEvaluator.Default.GetQuery(DbSet.AsQueryable(), specification);
    }
}