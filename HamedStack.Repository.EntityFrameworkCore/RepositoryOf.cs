using Ardalis.Specification;
using HamedStack.TheAggregateRoot.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using AutoMapper;
using Ardalis.Specification.EntityFrameworkCore;

namespace HamedStack.TheRepository.EntityFrameworkCore;

/// <summary>
/// Repository implementation for managing database operations for domain models and their respective database models.
/// </summary>
/// <typeparam name="TDomainModel">The domain model type.</typeparam>
/// <typeparam name="TDatabaseModel">The database model type.</typeparam>
public class Repository<TDomainModel, TDatabaseModel> : IRepository<TDomainModel, TDatabaseModel>
    where TDatabaseModel : class
{
    private readonly DbContextBase _dbContext;
    private readonly TimeProvider _timeProvider;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="Repository{TDomainModel, TDatabaseModel}"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="mapper">The mapper for converting between domain and database models.</param>
    /// <param name="timeProvider">The time provider for handling audit timestamps.</param>
    public Repository(DbContextBase dbContext, IMapper mapper, TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
        _mapper = mapper;
    }

    /// <summary>
    /// Provides an <see cref="IQueryable{T}"/> for querying the database model entities.
    /// </summary>
    public virtual IQueryable<TDatabaseModel> Query => DbSet.AsQueryable();

    /// <summary>
    /// Gets the unit of work associated with the repository.
    /// </summary>
    public virtual IUnitOfWork UnitOfWork => _dbContext;

    /// <summary>
    /// Gets the underlying database context.
    /// </summary>
    protected virtual DbContextBase DbContext => _dbContext;

    /// <summary>
    /// Gets the DbSet for the database model.
    /// </summary>
    protected virtual DbSet<TDatabaseModel> DbSet => _dbContext.Set<TDatabaseModel>();

    /// <summary>
    /// Adds a domain model entity asynchronously to the database.
    /// </summary>
    /// <param name="entity">The domain model entity to add.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The added domain model entity.</returns>
    public virtual async Task<TDomainModel> AddAsync(TDomainModel entity, CancellationToken cancellationToken = default)
    {
        var dbEntity = _mapper.Map<TDatabaseModel>(entity);
        ApplyCreatedAuditTime(dbEntity);
        await DbSet.AddAsync(dbEntity, cancellationToken);
        return entity;
    }

    /// <summary>
    /// Adds a range of domain model entities asynchronously to the database.
    /// </summary>
    /// <param name="entities">The collection of domain model entities to add.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The added domain model entities.</returns>
    public virtual async Task<IEnumerable<TDomainModel>> AddRangeAsync(IEnumerable<TDomainModel> entities, CancellationToken cancellationToken = default)
    {
        var result = new List<TDomainModel>();
        foreach (var entity in entities)
        {
            var output = await AddAsync(entity, cancellationToken);
            result.Add(output);
        }
        return result;
    }

    /// <summary>
    /// Checks if any entity in the database matches the provided specification.
    /// </summary>
    /// <param name="specification">The specification to filter the query.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns><c>true</c> if any entities match the specification; otherwise, <c>false</c>.</returns>
    public virtual async Task<bool> AnyAsync(ISpecification<TDatabaseModel> specification, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification, true).AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if any entity in the database matches the provided predicate.
    /// </summary>
    /// <param name="predicate">The predicate to filter the query.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns><c>true</c> if any entities match the predicate; otherwise, <c>false</c>.</returns>
    public virtual async Task<bool> AnyAsync(Expression<Func<TDatabaseModel, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Counts the number of entities in the database that match the provided specification.
    /// </summary>
    /// <param name="specification">The specification to filter the query.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The count of matching entities.</returns>
    public virtual async Task<int> CountAsync(ISpecification<TDatabaseModel> specification, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification, true).CountAsync(cancellationToken);
    }

    /// <summary>
    /// Counts the total number of entities in the database.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The total count of entities.</returns>
    public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(cancellationToken);
    }

    /// <summary>
    /// Counts the number of entities in the database that match the provided predicate.
    /// </summary>
    /// <param name="predicate">The predicate to filter the query.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The count of matching entities.</returns>
    public virtual async Task<int> CountAsync(Expression<Func<TDatabaseModel, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Deletes a domain model entity asynchronously from the database.
    /// </summary>
    /// <param name="entity">The domain model entity to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public virtual async Task DeleteAsync(TDomainModel entity, CancellationToken cancellationToken = default)
    {
        var dbEntity = _mapper.Map<TDatabaseModel>(entity);
        DbSet.Remove(dbEntity);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Deletes an entity by its ID asynchronously from the database.
    /// </summary>
    /// <typeparam name="TKey">The type of the ID.</typeparam>
    /// <param name="id">The ID of the entity to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public virtual async Task DeleteAsync<TKey>(TKey id, CancellationToken cancellationToken = default) where TKey : notnull
    {
        if (!typeof(IIdentifier<TKey>).IsAssignableFrom(typeof(TDatabaseModel)))
        {
            throw new InvalidOperationException($"Entity type {typeof(TDatabaseModel).FullName} does not implement {typeof(IIdentifier<TKey>).FullName} interface.");
        }
        var entity = await DbSet.FindAsync(new object[] { id }, cancellationToken);
        if (entity != null) DbSet.Remove(entity);
    }

    /// <summary>
    /// Deletes an entity by its composite key asynchronously from the database.
    /// </summary>
    /// <param name="ids">The composite keys of the entity to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public virtual async Task DeleteAsync(object[] ids, CancellationToken cancellationToken = default)
    {
        var entity = await DbSet.FindAsync(ids, cancellationToken);
        if (entity != null) DbSet.Remove(entity);
    }

    /// <summary>
    /// Deletes a range of domain model entities asynchronously from the database.
    /// </summary>
    /// <param name="entities">The collection of domain model entities to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public virtual async Task DeleteRangeAsync(IEnumerable<TDomainModel> entities, CancellationToken cancellationToken = default)
    {
        var dbEntities = _mapper.Map<IEnumerable<TDatabaseModel>>(entities);
        DbSet.RemoveRange(dbEntities);
        await Task.CompletedTask; // consider using await DbContext.SaveChangesAsync(cancellationToken) here to persist changes
    }

    /// <summary>
    /// Deletes entities matching the provided specification asynchronously from the database.
    /// </summary>
    /// <param name="specification">The specification to filter the entities to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public virtual async Task DeleteRangeAsync(ISpecification<TDatabaseModel> specification, CancellationToken cancellationToken = default)
    {
        var query = ApplySpecification(specification);
        await DeleteRangeAsync(await GetAll(query, cancellationToken), cancellationToken);
    }

    /// <summary>
    /// Deletes a range of entities from the provided query asynchronously from the database.
    /// </summary>
    /// <param name="query">The query to filter the entities to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public virtual async Task DeleteRangeAsync(IQueryable<TDatabaseModel> query, CancellationToken cancellationToken = default)
    {
        var entities = await GetAll(query, cancellationToken);
        await DeleteRangeAsync(entities, cancellationToken);
    }

    /// <summary>
    /// Retrieves the first entity that matches the provided specification asynchronously.
    /// </summary>
    /// <param name="specification">The specification to filter the query.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The first matching domain model entity or <c>null</c> if no entity matches.</returns>
    public virtual async Task<TDomainModel?> FirstOrDefaultAsync(ISpecification<TDatabaseModel> specification, CancellationToken cancellationToken = default)
    {
        var dbEntity = await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
        return _mapper.Map<TDomainModel>(dbEntity);
    }

    /// <summary>
    /// Retrieves the first entity that matches the provided predicate asynchronously.
    /// </summary>
    /// <param name="predicate">The predicate to filter the query.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The first matching domain model entity or <c>null</c> if no entity matches.</returns>
    public virtual async Task<TDomainModel?> FirstOrDefaultAsync(Expression<Func<TDatabaseModel, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var dbEntity = await DbSet.FirstOrDefaultAsync(predicate, cancellationToken);
        return _mapper.Map<TDomainModel>(dbEntity);
    }

    /// <summary>
    /// Retrieves all domain model entities asynchronously from the database.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A list of all domain model entities.</returns>
    public virtual async Task<List<TDomainModel>> GetAll(CancellationToken cancellationToken = default)
    {
        var dbEntities = await DbSet.ToListAsync(cancellationToken);
        return _mapper.Map<List<TDomainModel>>(dbEntities);
    }

    /// <summary>
    /// Retrieves all domain model entities that match the provided specification asynchronously.
    /// </summary>
    /// <param name="specification">The specification to filter the query.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A list of matching domain model entities.</returns>
    public virtual async Task<List<TDomainModel>> GetAll(ISpecification<TDatabaseModel> specification, CancellationToken cancellationToken = default)
    {
        var dbEntities = await ApplySpecification(specification).ToListAsync(cancellationToken);
        return _mapper.Map<List<TDomainModel>>(dbEntities);
    }

    /// <summary>
    /// Retrieves all entities as the specified result type that match the provided specification asynchronously.
    /// </summary>
    /// <typeparam name="TResult">The result type of the query.</typeparam>
    /// <param name="specification">The specification to filter the query.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A list of matching result entities.</returns>
    public virtual async Task<List<TResult>> GetAll<TResult>(ISpecification<TDatabaseModel, TResult> specification, CancellationToken cancellationToken = default)
    {
        var queryResult = await ApplySpecification(specification).ToListAsync(cancellationToken);
        return specification.PostProcessingAction == null ? queryResult : specification.PostProcessingAction(queryResult).ToList();
    }

    /// <summary>
    /// Retrieves all domain model entities from a provided query asynchronously.
    /// </summary>
    /// <param name="query">The query to execute.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A list of matching domain model entities.</returns>
    public virtual async Task<List<TDomainModel>> GetAll(IQueryable<TDatabaseModel> query, CancellationToken cancellationToken = default)
    {
        var dbEntities = await query.ToListAsync(cancellationToken);
        return _mapper.Map<List<TDomainModel>>(dbEntities);
    }

    /// <summary>
    /// Retrieves an asynchronous enumerable of domain model entities.
    /// </summary>
    /// <returns>An asynchronous enumerable of domain model entities.</returns>
    public virtual IAsyncEnumerable<TDomainModel> GetAsyncEnumerable()
    {
        return _mapper.ProjectTo<TDomainModel>(DbSet.AsQueryable()).AsAsyncEnumerable();
    }

    /// <summary>
    /// Retrieves an asynchronous enumerable of domain model entities matching the provided specification.
    /// </summary>
    /// <param name="specification">The specification to filter the query.</param>
    /// <returns>An asynchronous enumerable of matching domain model entities.</returns>
    public virtual IAsyncEnumerable<TDomainModel> GetAsyncEnumerable(ISpecification<TDatabaseModel> specification)
    {
        var query = ApplySpecification(specification);
        return _mapper.ProjectTo<TDomainModel>(query).AsAsyncEnumerable();
    }

    /// <summary>
    /// Retrieves an entity by its ID asynchronously.
    /// </summary>
    /// <typeparam name="TKey">The type of the ID.</typeparam>
    /// <param name="id">The ID of the entity to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The matching domain model entity or <c>null</c> if no entity matches.</returns>
    public virtual async ValueTask<TDomainModel?> GetByIdAsync<TKey>(TKey id, CancellationToken cancellationToken = default) where TKey : notnull
    {
        if (!typeof(IIdentifier<TKey>).IsAssignableFrom(typeof(TDatabaseModel)))
        {
            throw new InvalidOperationException($"Entity type {typeof(TDatabaseModel).FullName} does not implement {typeof(IIdentifier<TKey>).FullName} interface.");
        }
        var dbEntity = await DbSet.FindAsync(new object[] { id }, cancellationToken);
        return _mapper.Map<TDomainModel>(dbEntity);
    }

    /// <summary>
    /// Retrieves an entity by its composite keys asynchronously.
    /// </summary>
    /// <param name="ids">The composite keys of the entity to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The matching domain model entity or <c>null</c> if no entity matches.</returns>
    public virtual async ValueTask<TDomainModel?> GetByIdsAsync(object[] ids, CancellationToken cancellationToken = default)
    {
        var dbEntity = await DbSet.FindAsync(ids, cancellationToken);
        return _mapper.Map<TDomainModel>(dbEntity);
    }

    /// <summary>
    /// Retrieves a paginated list of domain model entities asynchronously.
    /// </summary>
    /// <param name="pageNumber">The page number to retrieve.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A paginated list of domain model entities.</returns>
    public virtual async Task<List<TDomainModel>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var dbEntities = await DbSet.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return _mapper.Map<List<TDomainModel>>(dbEntities);
    }

    /// <summary>
    /// Retrieves a paginated list of domain model entities matching the provided specification asynchronously.
    /// </summary>
    /// <param name="specification">The specification to filter the query.</param>
    /// <param name="pageNumber">The page number to retrieve.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A paginated list of matching domain model entities.</returns>
    public virtual async Task<List<TDomainModel>> GetPagedAsync(ISpecification<TDatabaseModel> specification, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = ApplySpecification(specification);
        var dbEntities = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return _mapper.Map<List<TDomainModel>>(dbEntities);
    }

    /// <summary>
    /// Retrieves a paginated list of domain model entities from a provided query asynchronously.
    /// </summary>
    /// <param name="query">The query to execute.</param>
    /// <param name="pageNumber">The page number to retrieve.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A paginated list of matching domain model entities.</returns>
    public virtual async Task<List<TDomainModel>> GetPagedAsync(IQueryable<TDatabaseModel> query, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var dbEntities = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return _mapper.Map<List<TDomainModel>>(dbEntities);
    }

    /// <summary>
    /// Retrieves a single entity matching the provided specification asynchronously.
    /// </summary>
    /// <param name="specification">The specification to filter the query.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The matching domain model entity or <c>null</c> if no entity matches.</returns>
    public virtual async Task<TDomainModel?> SingleOrDefaultAsync(ISpecification<TDatabaseModel> specification, CancellationToken cancellationToken = default)
    {
        var dbEntity = await ApplySpecification(specification).SingleOrDefaultAsync(cancellationToken);
        return _mapper.Map<TDomainModel>(dbEntity);
    }

    /// <summary>
    /// Retrieves a single entity matching the provided predicate asynchronously.
    /// </summary>
    /// <param name="predicate">The predicate to filter the query.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The matching domain model entity or <c>null</c> if no entity matches.</returns>
    public virtual async Task<TDomainModel?> SingleOrDefaultAsync(Expression<Func<TDatabaseModel, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var dbEntity = await DbSet.SingleOrDefaultAsync(predicate, cancellationToken);
        return _mapper.Map<TDomainModel>(dbEntity);
    }

    /// <summary>
    /// Updates a domain model entity asynchronously in the database.
    /// </summary>
    /// <param name="entity">The domain model entity to update.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public virtual Task UpdateAsync(TDomainModel entity, CancellationToken cancellationToken = default)
    {
        var dbEntity = _mapper.Map<TDatabaseModel>(entity);
        ApplyModifiedAuditTime(dbEntity);
        DbSet.Update(dbEntity);
        return Task.CompletedTask; // consider using await DbContext.SaveChangesAsync(cancellationToken) here to persist changes
    }

    /// <summary>
    /// Updates a range of domain model entities asynchronously in the database.
    /// </summary>
    /// <param name="entities">The collection of domain model entities to update.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public virtual async Task UpdateRangeAsync(IEnumerable<TDomainModel> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
        {
            await UpdateAsync(entity, cancellationToken);
        }
    }

    /// <summary>
    /// Applies the created audit timestamp to the provided database entity.
    /// </summary>
    /// <param name="entity">The database model entity to update.</param>
    protected virtual void ApplyCreatedAuditTime(TDatabaseModel entity)
    {
        if (entity is IAudit audit)
        {
            audit.CreatedOn = _timeProvider.GetUtcNow();
            audit.CreatedBy = ToString();
        }
    }

    /// <summary>
    /// Applies the modified audit timestamp to the provided database entity.
    /// </summary>
    /// <param name="entity">The database model entity to update.</param>
    protected virtual void ApplyModifiedAuditTime(TDatabaseModel entity)
    {
        if (entity is IAudit audit)
        {
            audit.ModifiedOn = _timeProvider.GetUtcNow();
            audit.ModifiedBy = ToString();
        }
    }

    /// <summary>
    /// Applies the provided specification to filter the query.
    /// </summary>
    /// <param name="specification">The specification to apply.</param>
    /// <param name="evaluateCriteriaOnly">Indicates whether to apply only the criteria from the specification.</param>
    /// <returns>The filtered queryable object.</returns>
    protected virtual IQueryable<TDatabaseModel> ApplySpecification(ISpecification<TDatabaseModel> specification, bool evaluateCriteriaOnly = false)
    {
        return SpecificationEvaluator.Default.GetQuery(DbSet.AsQueryable(), specification, evaluateCriteriaOnly);
    }

    /// <summary>
    /// Applies the provided specification to filter the query and project it to the specified result type.
    /// </summary>
    /// <typeparam name="TResult">The result type to project to.</typeparam>
    /// <param name="specification">The specification to apply.</param>
    /// <returns>The filtered and projected queryable object.</returns>
    protected virtual IQueryable<TResult> ApplySpecification<TResult>(ISpecification<TDatabaseModel, TResult> specification)
    {
        return SpecificationEvaluator.Default.GetQuery(DbSet.AsQueryable(), specification);
    }
}

