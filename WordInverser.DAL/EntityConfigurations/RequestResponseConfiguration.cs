using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordInverser.Common.Interfaces;
using WordInverser.DAL.Entities;

namespace WordInverser.DAL.EntityConfigurations;

public class RequestResponseConfiguration : IWordInverserEntityTypeConfiguration<RequestResponse>
{
    public void Configure(EntityTypeBuilder<RequestResponse> builder)
    {
        builder.ToTable("RequestResponse");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.RequestId).IsRequired().HasDefaultValueSql("NEWID()");
        builder.Property(e => e.Request).IsRequired();
        builder.Property(e => e.Response).IsRequired();
        builder.Property(e => e.Tags).IsRequired();
        builder.Property(e => e.IsSuccess).IsRequired().HasDefaultValue(true);
        builder.Property(e => e.CreatedDate).IsRequired().HasDefaultValueSql("GETUTCDATE()");
        builder.HasIndex(e => e.CreatedDate).HasDatabaseName("IX_RequestResponse_CreatedDate");
        builder.HasIndex(e => e.IsSuccess).HasDatabaseName("IX_RequestResponse_IsSuccess");
        builder.HasIndex(e => e.RequestId).IsUnique();
    }
}
