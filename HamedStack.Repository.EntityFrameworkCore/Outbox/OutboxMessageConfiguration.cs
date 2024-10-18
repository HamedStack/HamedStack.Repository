using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HamedStack.TheRepository.EntityFrameworkCore.Outbox;

/// <summary>
/// Provides configuration for the <see cref="OutboxMessage"/> entity in the Entity Framework Core model.
/// This configuration maps the entity to the "OutboxMessages" table.
/// </summary>
public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    /// <summary>
    /// Configures the entity type for <see cref="OutboxMessage"/>, specifying the table name.
    /// </summary>
    /// <param name="builder">The <see cref="EntityTypeBuilder{OutboxMessage}"/> used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
    }
}
