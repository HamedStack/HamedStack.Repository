using System.Data;
using System.Linq.Expressions;
using System.Text.Json;
using HamedStack.TheAggregateRoot.Abstractions;
using HamedStack.TheAggregateRoot.Events;
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
    private readonly List<DomainEvent> _domainEvents = new();
    private readonly ILogger<DbContextBase> _logger;
    private IDbContextTransaction? _dbContextTransaction;

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
    /// Gets or sets the <see cref="DbSet{OutboxMessage}"/> used to store outbox messages.
    /// </summary>
    internal DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

    /// <summary>
    /// Adds one or more domain events to the internal collection.
    /// </summary>
    /// <param name="domainEvents">An array of <see cref="DomainEvent"/> to add.</param>
    public void AddDomainEvents(params DomainEvent[] domainEvents)
    {
        _domainEvents.AddRange(domainEvents);
    }

    /// <summary>
    /// Adds a collection of domain events to the internal collection.
    /// </summary>
    /// <param name="domainEvents">An <see cref="IEnumerable{T}"/> of <see cref="DomainEvent"/> to add.</param>
    public void AddDomainEvents(IEnumerable<DomainEvent> domainEvents)
    {
        _domainEvents.AddRange(domainEvents);
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
    /// Saves changes made in the context to the database and applies domain events to the outbox.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    public override int SaveChanges()
    {
        ApplyDomainEvents();
        return base.SaveChanges();
    }

    /// <summary>
    /// Saves changes made in the context to the database with an option to accept all changes, and applies domain events to the outbox.
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess">
    /// A boolean value indicating whether all changes should be accepted if the operation is successful.
    /// </param>
    /// <returns>The number of state entries written to the database.</returns>
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyDomainEvents();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    /// <summary>
    /// Asynchronously saves changes made in the context to the database and applies domain events to the outbox.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token to observe while waiting for the task to complete.
    /// </param>
    /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        ApplyDomainEvents();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Asynchronously saves changes made in the context to the database with an option to accept all changes, and applies domain events to the outbox.
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess">
    /// A boolean value indicating whether all changes should be accepted if the operation is successful.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to observe while waiting for the task to complete.
    /// </param>
    /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
    {
        ApplyDomainEvents();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// Configures the conventions for the model by adding a custom convention to automatically add blank triggers.
    /// </summary>
    /// <param name="configurationBuilder">The <see cref="ModelConfigurationBuilder"/> used to configure the model conventions.</param>
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Conventions.Add(_ => new BlankTriggerAddingConvention());
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

    /// <summary>
    /// Applies all pending domain events by converting them into outbox messages and clearing the internal collection.
    /// </summary>
    /// <remarks>
    /// Each domain event is serialized and stored as an <see cref="OutboxMessage"/> for processing.
    /// </remarks>
    private void ApplyDomainEvents()
    {
        foreach (var domainEvent in _domainEvents)
        {
            OutboxMessages.Add(new OutboxMessage()
            {
                Id = Guid.NewGuid(),
                Name = domainEvent.GetType().Name,
                Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                CreatedOn = DateTime.Now,
                IsProcessed = false,
                ProcessedOn = null,
            });
        }

        _domainEvents.Clear();
    }
}