using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordInverser.Common.Interfaces;
using WordInverser.DAL.Entities;

namespace WordInverser.DAL.EntityConfigurations;

public class WordCacheConfiguration : IWordInverserEntityTypeConfiguration<WordCache>
{
    public void Configure(EntityTypeBuilder<WordCache> builder)
    {
        builder.ToTable("WordCache");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Word).IsRequired().HasMaxLength(500);
        builder.Property(e => e.InversedWord).IsRequired().HasMaxLength(500);
        builder.Property(e => e.CreatedDate).IsRequired().HasDefaultValueSql("GETUTCDATE()");
        builder.Property(e => e.UpdatedDate).IsRequired().HasDefaultValueSql("GETUTCDATE()");
        builder.HasIndex(e => e.Word).IsUnique().HasDatabaseName("IX_WordCache_Word");
    }
}
