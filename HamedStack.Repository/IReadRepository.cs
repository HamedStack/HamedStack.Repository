using System.Linq.Expressions;
using HamedStack.Specification;

namespace HamedStack.TheRepository;

public interface IReadRepository<TEntity> where TEntity : class
{
    IQueryable<TEntity> Query { get; }
    Task<bool> AllAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    Task<bool> AllAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    IAsyncEnumerable<TEntity> AsAsyncEnumerable(ISpecification<TEntity> specification);

    IAsyncEnumerable<TEntity> AsAsyncEnumerable(IQueryable<TEntity> query);

    Task<int> CountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    Task<int> CountAsync(CancellationToken cancellationToken = default);

    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task<TEntity?> FirstOrDefaultAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    ValueTask<TEntity?> GetByIdAsync<TKey>(TKey id, CancellationToken cancellationToken = default) where TKey : notnull;
    ValueTask<TEntity?> GetByIdsAsync(object[] ids, CancellationToken cancellationToken = default);

    Task<List<TEntity>> ToListAsync(CancellationToken cancellationToken = default);

    Task<List<TEntity>> ToListAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    Task<List<TEntity>> ToListAsync(IQueryable<TEntity> query, CancellationToken cancellationToken = default);

    Task<TEntity?> SingleOrDefaultAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);
    Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    IQueryable<TEntity> ToQueryable(ISpecification<TEntity> specification);
}