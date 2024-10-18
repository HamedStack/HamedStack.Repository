using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace HamedStack.TheRepository.EntityFrameworkCore;

/// <summary>
/// Represents a convention that adds blank triggers to entity types during the model finalizing stage.
/// </summary>
public class BlankTriggerAddingConvention : IModelFinalizingConvention
{
    /// <summary>
    /// Processes the model during finalization to ensure that triggers are added to the appropriate entity types.
    /// </summary>
    /// <param name="modelBuilder">The <see cref="IConventionModelBuilder"/> used to build the model.</param>
    /// <param name="context">The <see cref="IConventionContext{T}"/> that provides the context for the convention.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="modelBuilder"/> or <paramref name="context"/> is null.</exception>
    public virtual void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));
        if (context == null) throw new ArgumentNullException(nameof(context));
        
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            var table = StoreObjectIdentifier.Create(entityType, StoreObjectType.Table);
            if (table != null
                && entityType.GetDeclaredTriggers().All(t => t.GetDatabaseName(table.Value) == null)
                && (entityType.BaseType == null
                    || entityType.GetMappingStrategy() != RelationalAnnotationNames.TphMappingStrategy))
            {
                entityType.Builder.HasTrigger(table.Value.Name + "_Trigger");
            }
            foreach (var fragment in entityType.GetMappingFragments(StoreObjectType.Table))
            {
                if (entityType.GetDeclaredTriggers().All(t => t.GetDatabaseName(fragment.StoreObject) == null))
                {
                    entityType.Builder.HasTrigger(fragment.StoreObject.Name + "_Trigger");
                }
            }
        }
    }
}
