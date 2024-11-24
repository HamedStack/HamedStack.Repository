using HamedStack.TheAggregateRoot.Events;
using HamedStack.TheRepository.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HamedStack.TheRepository.ServiceCollection;

/// <summary>
/// Provides extension methods for registering infrastructure services in the dependency injection container.
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Registers infrastructure services for the application, including database context, unit of work, repositories, 
    /// and domain event dispatcher.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the database context that derives from <see cref="DbContextBase"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which services are added.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> for chaining.</returns>
    /// <remarks>
    /// - Registers <see cref="TimeProvider.System"/> as a singleton.
    /// - Registers <typeparamref name="TDbContext"/> as a scoped service.
    /// - Maps <see cref="DbContextBase"/> and <see cref="IUnitOfWork"/> to <typeparamref name="TDbContext"/>.
    /// - Registers generic repositories implementing <see cref="IRepository{T}"/>.
    /// - Registers <see cref="IDomainEventDispatcher"/> for dispatching domain events.
    /// </remarks>
    public static IServiceCollection AddInfrastructureServices<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContextBase
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<TDbContext>();
        services.AddScoped<DbContextBase>(provider => provider.GetRequiredService<TDbContext>());
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<TDbContext>());
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        return services;
    }
}
