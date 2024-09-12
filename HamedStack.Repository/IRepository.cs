using HamedStack.Specification;

namespace HamedStack.TheRepository;

public interface IRepository<TEntity> : IReadRepository<TEntity> where TEntity : class
{
    IUnitOfWork UnitOfWork { get; }

    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    Task DeleteAsync(object id, CancellationToken cancellationToken = default);

    Task DeleteRangeAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(IQueryable<TEntity> query, CancellationToken cancellationToken = default);

}