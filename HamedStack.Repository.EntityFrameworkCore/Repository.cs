using System.Linq.Expressions;
using HamedStack.Specification;
using HamedStack.TheAggregateRoot.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace HamedStack.TheRepository.EntityFrameworkCore;

public class Repository<TEntity> : IRepository<TEntity>
    where TEntity : class
{
    private readonly DbContextBase _dbContext;
    private readonly TimeProvider _timeProvider;
    public Repository(DbContextBase dbContext, TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public virtual IUnitOfWork UnitOfWork => _dbContext;
    protected virtual DbSet<TEntity> DbSet => _dbContext.Set<TEntity>();
    public virtual IQueryable<TEntity> Query => DbSet.AsQueryable();

    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var hasAudit = entity is IAudit;
        if (hasAudit)
        {
            (entity as IAudit)!.CreatedOn = _timeProvider.GetUtcNow();
            (entity as IAudit)!.CreatedBy = ToString();
        }

        await DbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public virtual async Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        var result = new List<TEntity>();
        foreach (var entity in entities)
        {
            var output = await AddAsync(entity, cancellationToken);
            result.Add(output);
        }
        return result;
    }

    public virtual Task<bool> AllAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        return DbSet.AllAsync(specification.ToExpression(), cancellationToken);

    }

    public virtual Task<bool> AllAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return DbSet.AllAsync(predicate, cancellationToken);
    }

    public virtual Task<bool> AnyAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        return DbSet.AnyAsync(specification.ToExpression(), cancellationToken);
    }

    public virtual Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return DbSet.AnyAsync(predicate, cancellationToken);
    }

    public virtual IAsyncEnumerable<TEntity> AsAsyncEnumerable(ISpecification<TEntity> specification)
    {
        return ToQueryable(specification).AsAsyncEnumerable();
    }

    public virtual IAsyncEnumerable<TEntity> AsAsyncEnumerable(IQueryable<TEntity> query)
    {
        return query.AsAsyncEnumerable();
    }

    public virtual Task<int> CountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        return DbSet.CountAsync(specification.ToExpression(), cancellationToken);
    }

    public virtual Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return DbSet.CountAsync(cancellationToken);
    }

    public virtual Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return DbSet.CountAsync(predicate, cancellationToken);
    }

    public virtual Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        DbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public virtual Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        DbSet.RemoveRange(entities);
        return Task.CompletedTask;
    }

    public virtual Task DeleteAsync(object id, CancellationToken cancellationToken = default)
    {
        var entity = DbSet.Find(id);
        return entity != null ? DeleteAsync(entity, cancellationToken) : Task.CompletedTask;
    }

    public virtual async Task DeleteRangeAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        var entities = await ToQueryable(specification).ToListAsync(cancellationToken);
        DbSet.RemoveRange(entities);
    }

    public virtual async Task DeleteRangeAsync(IQueryable<TEntity> query, CancellationToken cancellationToken = default)
    {
        var entities = await ToListAsync(query, cancellationToken);
        await DeleteRangeAsync(entities, cancellationToken);
    }

    public virtual Task<TEntity?> FirstOrDefaultAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        return DbSet.FirstOrDefaultAsync(specification.ToExpression(), cancellationToken);
    }

    public virtual Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return DbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual ValueTask<TEntity?> GetByIdAsync<TKey>(TKey id, CancellationToken cancellationToken = default) where TKey : notnull
    {
        if (!typeof(IIdentifier<TKey>).IsAssignableFrom(typeof(TEntity)))
        {
            throw new InvalidOperationException($"Entity type {typeof(TEntity).FullName} does not implement {typeof(IIdentifier<TKey>).FullName} interface.");
        }
        return DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual ValueTask<TEntity?> GetByIdsAsync(object[] ids, CancellationToken cancellationToken = default)
    {
        return DbSet.FindAsync(ids, cancellationToken);
    }

    public virtual Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return DbSet.SingleOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual Task<TEntity?> SingleOrDefaultAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        return DbSet.SingleOrDefaultAsync(specification.ToExpression(), cancellationToken);
    }

    public virtual Task<List<TEntity>> ToListAsync(CancellationToken cancellationToken = default)
    {
        return DbSet.ToListAsync(cancellationToken);
    }

    public virtual Task<List<TEntity>> ToListAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        return ToQueryable(specification).ToListAsync(cancellationToken);
    }

    public virtual Task<List<TEntity>> ToListAsync(IQueryable<TEntity> query, CancellationToken cancellationToken = default)
    {
        return query.ToListAsync(cancellationToken);
    }

    public virtual IQueryable<TEntity> ToQueryable(ISpecification<TEntity> specification)
    {
        return DbSet.Where(specification.ToExpression());
    }

    public virtual Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var hasAudit = entity is IAudit;
        if (hasAudit)
        {
            (entity as IAudit)!.ModifiedOn = _timeProvider.GetUtcNow();
            (entity as IAudit)!.ModifiedBy = ToString();
        }

        DbSet.Update(entity);
        return Task.CompletedTask;
    }

    public virtual async Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
        {
            await UpdateAsync(entity, cancellationToken);
        }
    }
}