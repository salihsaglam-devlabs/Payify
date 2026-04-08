using LinkPara.Card.Domain.Entities.Archive;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.Archive;

public class ArchiveBatchConfiguration : IEntityTypeConfiguration<ArchiveBatch>
{
    public void Configure(EntityTypeBuilder<ArchiveBatch> builder)
    {
        builder.Property(x => x.RequestedAt).HasColumnName("requested_at");
        builder.Property(x => x.StartedAt).HasColumnName("started_at");
        builder.Property(x => x.CompletedAt).HasColumnName("completed_at");
        builder.Property(x => x.RequestedBy).HasColumnName("requested_by").HasMaxLength(100).IsRequired();
        builder.Property(x => x.FilterJson).HasColumnName("filter_json");
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(32).IsRequired();
        builder.Property(x => x.ProcessedCount).HasColumnName("processed_count");
        builder.Property(x => x.ArchivedCount).HasColumnName("archived_count");
        builder.Property(x => x.SkippedCount).HasColumnName("skipped_count");
        builder.Property(x => x.FailedCount).HasColumnName("failed_count");
    }
}
