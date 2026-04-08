using LinkPara.Card.Domain.Entities.Archive;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.Archive;

public class ArchiveIngestionFileLineConfiguration : IEntityTypeConfiguration<ArchiveIngestionFileLine>
{
    public void Configure(EntityTypeBuilder<ArchiveIngestionFileLine> builder)
    {
        builder.Property(x => x.IngestionFileId).HasColumnName("file_id");
        builder.Property(x => x.LineNumber).HasColumnName("line_number");
        builder.Property(x => x.ByteOffset).HasColumnName("byte_offset");
        builder.Property(x => x.ByteLength).HasColumnName("byte_length");
        builder.Property(x => x.RecordType).HasColumnName("line_type").HasMaxLength(8);
        builder.Property(x => x.RawData).HasColumnName("raw_content");
        builder.Property(x => x.ParsedData).HasColumnName("parsed_content");
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Message).HasColumnName("message").HasMaxLength(4000);
        builder.Property(x => x.RetryCount).HasColumnName("retry_count");
        builder.Property(x => x.CorrelationKey).HasColumnName("correlation_key").HasMaxLength(256);
        builder.Property(x => x.CorrelationValue).HasColumnName("correlation_value").HasMaxLength(1024);
        builder.Property(x => x.DuplicateDetectionKey).HasColumnName("duplicate_detection_key").HasMaxLength(256);
        builder.Property(x => x.DuplicateStatus).HasColumnName("duplicate_status").HasMaxLength(64);
        builder.Property(x => x.DuplicateGroupId).HasColumnName("duplicate_group_id");
        builder.Property(x => x.ReconciliationStatus).HasColumnName("reconciliation_status").HasMaxLength(32);
        builder.Property(x => x.ArchiveRunId).HasColumnName("archive_run_id");
        builder.Property(x => x.ArchivedAt).HasColumnName("archived_at");
        builder.Property(x => x.ArchivedBy).HasColumnName("archived_by").HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.IngestionFileId);
        builder.HasIndex(x => x.ArchiveRunId);
    }
}
