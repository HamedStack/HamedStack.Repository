using System.Data;
using System.Linq.Expressions;
using HamedStack.TheAggregateRoot.Abstractions;
using HamedStack.TheRepository.EntityFrameworkCore.Interceptors;
using HamedStack.TheRepository.EntityFrameworkCore.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace HamedStack.TheRepository.EntityFrameworkCore;

/// <summary>
/// Base class for DbContext that implements the <see cref="IUnitOfWork"/> pattern and provides transaction handling and interceptors for domain events, auditing, and soft deletes.
/// </summary>
public class DbContextBase : DbContext, IUnitOfWork
{
    private readonly ILogger<DbContextBase> _logger;
    private IDbContextTransaction? _dbContextTransaction;

    /// <summary>
    /// Gets or sets the <see cref="DbSet{OutboxMessage}"/> used to store outbox messages.
    /// </summary>
    internal DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbContextBase"/> class with the specified options and logger.
    /// </summary>
    /// <param name="options">The options for configuring the context.</param>
    /// <param name="logger">The logger instance for logging.</param>
    public DbContextBase(DbContextOptions options, ILogger<DbContextBase> logger)
        : base(options)
    {
        _logger = logger;
    }

    /// <summary>
    /// Begins a new database transaction asynchronously with the specified isolation level.
    /// </summary>
    /// <param name="isolationLevel">The isolation level for the transaction. Default is <see cref="IsolationLevel.ReadCommitted"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the <see cref="IDbTransaction"/> object for the transaction.</returns>
    public virtual async Task<IDbTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default)
    {
        _dbContextTransaction = await Database.BeginTransactionAsync(isolationLevel, cancellationToken);
        return _dbContextTransaction.GetDbTransaction();
    }

    /// <summary>
    /// Commits the current transaction asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_dbContextTransaction != null)
            await _dbContextTransaction.CommitAsync(cancellationToken);
    }

    /// <summary>
    /// Rolls back the current transaction asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_dbContextTransaction != null)
                await _dbContextTransaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            if (_dbContextTransaction != null)
            {
                _dbContextTransaction.Dispose();
                _dbContextTransaction = null;
            }
        }
    }

    /// <summary>
    /// Configures the database context with additional options and interceptors.
    /// </summary>
    /// <param name="optionsBuilder">A builder used to configure the context options.</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new DomainEventOutboxInterceptor());
        optionsBuilder.AddInterceptors(new SoftDeleteInterceptor());
        optionsBuilder.AddInterceptors(new AuditInterceptor());
        optionsBuilder.AddInterceptors(new PerformanceInterceptor(_logger));
    }

    /// <summary>
    /// Configures the model with custom configurations such as soft delete filters and row version handling.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for the context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        SetRowVersion(modelBuilder);
        SetSoftDeleteQueryFilter(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Sets up the row version property for entities implementing the <see cref="IRowVersion"/> interface.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for the context.</param>
    private static void SetRowVersion(ModelBuilder modelBuilder)
    {
        var entityTypes = modelBuilder.Model.GetEntityTypes()
            .Where(t => typeof(IRowVersion).IsAssignableFrom(t.ClrType));

        foreach (var entityType in entityTypes)
        {
            modelBuilder.Entity(entityType.ClrType)
                .Property("RowVersion")
                .IsRowVersion();
        }
    }

    /// <summary>
    /// Sets up a query filter for entities implementing the <see cref="ISoftDelete"/> interface to exclude deleted records.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for the context.</param>
    private static void SetSoftDeleteQueryFilter(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType)) continue;

            var entityClrType = entityType.ClrType;
            var parameter = Expression.Parameter(entityClrType, "e");
            var property = Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
            var filterExpression = Expression.Lambda(Expression.Equal(property, Expression.Constant(false)), parameter);
            modelBuilder.Entity(entityClrType).HasQueryFilter(filterExpression);
        }
    }
}
