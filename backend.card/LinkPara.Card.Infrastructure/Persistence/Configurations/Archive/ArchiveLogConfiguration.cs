using LinkPara.Card.Domain.Entities.Archive.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.Archive;

public class ArchiveLogConfiguration : IEntityTypeConfiguration<ArchiveLog>
{
    public void Configure(EntityTypeBuilder<ArchiveLog> builder)
    {
        builder.Property(x => x.IngestionFileId);
        builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Message).HasMaxLength(2000);
        builder.Property(x => x.FailureReasonsJson);
        builder.Property(x => x.FilterJson);
        builder.HasIndex(x => x.IngestionFileId);
    }
}

