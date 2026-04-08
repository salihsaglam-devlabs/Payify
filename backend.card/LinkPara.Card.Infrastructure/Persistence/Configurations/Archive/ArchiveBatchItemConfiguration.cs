using LinkPara.Card.Infrastructure.Persistence.ArchiveEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.Archive;

public class ArchiveBatchItemConfiguration : IEntityTypeConfiguration<ArchiveBatchItem>
{
    public void Configure(EntityTypeBuilder<ArchiveBatchItem> builder)
    {
        builder.Property(x => x.BatchId).HasColumnName("batch_id");
        builder.Property(x => x.IngestionFileId).HasColumnName("ingestion_file_id");
        builder.Property(x => x.ArchiveRunId).HasColumnName("archive_run_id");
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(32).IsRequired();
        builder.Property(x => x.Message).HasColumnName("message").HasMaxLength(2000);
        builder.Property(x => x.FailureReasonsJson).HasColumnName("failure_reasons_json");
        builder.Property(x => x.ProcessedAt).HasColumnName("processed_at");
        builder.HasIndex(x => x.BatchId);
        builder.HasIndex(x => x.IngestionFileId);
    }
}
