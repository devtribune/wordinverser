using Microsoft.EntityFrameworkCore;

namespace WordInverser.Common.Interfaces;

/// <summary>
/// Interface for entity type configuration that can be automatically discovered and applied.
/// Wraps EF Core's IEntityTypeConfiguration for dynamic registration.
/// </summary>
/// <typeparam name="TEntity">The entity type to configure</typeparam>
public interface IWordInverserEntityTypeConfiguration<TEntity> : Microsoft.EntityFrameworkCore.IEntityTypeConfiguration<TEntity> 
    where TEntity : class
{
}
