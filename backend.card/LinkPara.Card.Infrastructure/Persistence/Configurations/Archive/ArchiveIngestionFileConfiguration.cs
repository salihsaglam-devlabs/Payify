using LinkPara.Card.Domain.Entities.Archive;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.Archive;

public class ArchiveIngestionFileConfiguration : IEntityTypeConfiguration<ArchiveIngestionFile>
{
    public void Configure(EntityTypeBuilder<ArchiveIngestionFile> builder)
    {
        builder.Property(x => x.FileKey).HasColumnName("file_key").HasMaxLength(128).IsRequired();
        builder.Property(x => x.FileName).HasColumnName("file_name").HasMaxLength(256).IsRequired();
        builder.Property(x => x.FullPath).HasColumnName("file_path").HasMaxLength(1024).IsRequired();
        builder.Property(x => x.SourceType).HasColumnName("source_type").HasMaxLength(50).IsRequired();
        builder.Property(x => x.FileType).HasColumnName("file_type").HasMaxLength(50).IsRequired();
        builder.Property(x => x.ContentType).HasColumnName("content_type").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Message).HasColumnName("message").HasMaxLength(4000);
        builder.Property(x => x.ExpectedCount).HasColumnName("expected_line_count");
        builder.Property(x => x.TotalCount).HasColumnName("processed_line_count");
        builder.Property(x => x.SuccessCount).HasColumnName("successful_line_count");
        builder.Property(x => x.ErrorCount).HasColumnName("failed_line_count");
        builder.Property(x => x.LastProcessedLineNumber).HasColumnName("last_processed_line_number");
        builder.Property(x => x.LastProcessedByteOffset).HasColumnName("last_processed_byte_offset");
        builder.Property(x => x.IsArchived).HasColumnName("is_archived");
        builder.Property(x => x.ArchiveRunId).HasColumnName("archive_run_id");
        builder.Property(x => x.ArchivedAt).HasColumnName("archived_at");
        builder.Property(x => x.ArchivedBy).HasColumnName("archived_by").HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.ArchiveRunId);
    }
}
