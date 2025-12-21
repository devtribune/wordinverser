using System.Reflection;
using Microsoft.EntityFrameworkCore;
using WordInverser.Common.Interfaces;

namespace WordInverser.DAL.Context;

/// <summary>
/// Database context for Word Inverser application.
/// Entities are configured dynamically through IWordInverserEntityTypeConfiguration implementations.
/// 
/// To add a new entity:
/// 1. Create the entity class in WordInverser.DAL/Entities/
/// 2. Create a configuration class in WordInverser.DAL/EntityConfigurations/ implementing IWordInverserEntityTypeConfiguration<TEntity>
/// 3. The entity will be automatically discovered and configured - no changes needed to this DbContext!
/// 
/// Access entities using: context.GetDbSet<TEntity>() or context.Set<TEntity>()
/// </summary>
public class WordInverserDbContext : DbContext
{
    public WordInverserDbContext(DbContextOptions<WordInverserDbContext> options)
        : base(options)
    {
    }

    // Generic method to get DbSet for any entity type
    public DbSet<TEntity> GetDbSet<TEntity>() where TEntity : class
    {
        return Set<TEntity>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Dynamically discover and apply all entity configurations
        var configurationsAssembly = Assembly.GetExecutingAssembly();
        
        var configurationTypes = configurationsAssembly.GetTypes()
            .Where(type => !type.IsAbstract && !type.IsInterface)
            .Where(type => type.GetInterfaces().Any(i => 
                i.IsGenericType && 
                i.GetGenericTypeDefinition() == typeof(IWordInverserEntityTypeConfiguration<>)))
            .ToList();

        foreach (var configurationType in configurationTypes)
        {
            // Get the entity type from IWordInverserEntityTypeConfiguration<TEntity>
            var entityType = configurationType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IWordInverserEntityTypeConfiguration<>))
                .GetGenericArguments()[0];

            // Create an instance of the configuration
            var configurationInstance = Activator.CreateInstance(configurationType);

            // Get the Configure method
            var configureMethod = configurationType.GetMethod("Configure");

            // Get the Entity<TEntity>() method from ModelBuilder
            var entityMethod = typeof(ModelBuilder)
                .GetMethods()
                .First(m => m.Name == "Entity" && m.IsGenericMethodDefinition && m.GetParameters().Length == 0)
                .MakeGenericMethod(entityType);

            // Call Entity<TEntity>() to get EntityTypeBuilder<TEntity>
            var entityTypeBuilder = entityMethod.Invoke(modelBuilder, null);

            // Invoke Configure method with the entity type builder
            configureMethod?.Invoke(configurationInstance, new[] { entityTypeBuilder });
        }
    }
}
