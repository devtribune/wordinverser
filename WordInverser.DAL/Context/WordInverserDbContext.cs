using Microsoft.EntityFrameworkCore;
using WordInverser.DAL.Entities;

namespace WordInverser.DAL.Context;

public class WordInverserDbContext : DbContext
{
    public WordInverserDbContext(DbContextOptions<WordInverserDbContext> options)
        : base(options)
    {
    }

    public DbSet<WordCache> WordCaches { get; set; }
    public DbSet<RequestResponse> RequestResponses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // WordCache entity configuration
        modelBuilder.Entity<WordCache>(entity =>
        {
            entity.ToTable("WordCache");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Word).IsRequired().HasMaxLength(500);
            entity.Property(e => e.InversedWord).IsRequired().HasMaxLength(500);
            entity.Property(e => e.CreatedDate).IsRequired().HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedDate).IsRequired().HasDefaultValueSql("GETUTCDATE()");
            entity.HasIndex(e => e.Word).IsUnique().HasDatabaseName("IX_WordCache_Word");
        });

        // RequestResponse entity configuration
        modelBuilder.Entity<RequestResponse>(entity =>
        {
            entity.ToTable("RequestResponse");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RequestId).IsRequired().HasDefaultValueSql("NEWID()");
            entity.Property(e => e.Request).IsRequired();
            entity.Property(e => e.Response).IsRequired();
            entity.Property(e => e.Tags).IsRequired();
            entity.Property(e => e.IsSuccess).IsRequired().HasDefaultValue(true);
            entity.Property(e => e.CreatedDate).IsRequired().HasDefaultValueSql("GETUTCDATE()");
            entity.HasIndex(e => e.CreatedDate).HasDatabaseName("IX_RequestResponse_CreatedDate");
            entity.HasIndex(e => e.IsSuccess).HasDatabaseName("IX_RequestResponse_IsSuccess");
            entity.HasIndex(e => e.RequestId).IsUnique();
        });
    }
}
