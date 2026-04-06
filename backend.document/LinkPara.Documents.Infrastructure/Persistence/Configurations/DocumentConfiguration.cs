using LinkPara.Documents.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Documents.Infrastructure.Persistence.Configurations;
public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.Property(t => t.OriginalFileName).HasMaxLength(400).IsRequired();
        builder.Property(t => t.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(t => t.FilePath).HasMaxLength(1000).IsRequired();
    }
}