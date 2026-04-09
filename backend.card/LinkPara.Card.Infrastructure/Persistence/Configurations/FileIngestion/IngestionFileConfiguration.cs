using LinkPara.Card.Domain.Entities.FileIngestion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.FileIngestion;

public class IngestionFileConfiguration : IEntityTypeConfiguration<IngestionFile>
{
    public void Configure(EntityTypeBuilder<IngestionFile> builder)
    {
        ConfigureColumns(builder);

        builder.HasIndex(x => new { x.FileKey, x.FileName, x.SourceType, x.FileType })
            .IsUnique();
        builder.HasIndex(x => x.Status);
    }

    internal static void ConfigureColumns<T>(EntityTypeBuilder<T> builder) where T : IngestionFile
    {
        builder.Property(x => x.FileKey).HasColumnName("file_key").HasMaxLength(128).IsRequired();
        builder.Property(x => x.FileName).HasColumnName("file_name").HasMaxLength(256).IsRequired();
        builder.Property(x => x.FullPath).HasColumnName("file_path").HasMaxLength(1024).IsRequired();
        builder.Property(x => x.SourceType).HasColumnName("source_type").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.FileType).HasColumnName("file_type").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.ContentType).HasColumnName("content_type").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Message).HasColumnName("message").HasMaxLength(4000);
        builder.Property(x => x.ExpectedCount).HasColumnName("expected_line_count");
        builder.Property(x => x.TotalCount).HasColumnName("processed_line_count");
        builder.Property(x => x.SuccessCount).HasColumnName("successful_line_count");
        builder.Property(x => x.ErrorCount).HasColumnName("failed_line_count");
        builder.Property(x => x.LastProcessedLineNumber).HasColumnName("last_processed_line_number");
        builder.Property(x => x.LastProcessedByteOffset).HasColumnName("last_processed_byte_offset");
        builder.Property(x => x.IsArchived).HasColumnName("is_archived");
        builder.Property(x => x.ArchiveRecordWrittenAt).HasColumnName("archive_record_written_at");
        builder.Property(x => x.ArchiveRecordRunId).HasColumnName("archive_record_run_id");
        builder.Property(x => x.ArchiveChildrenTransitionedAt).HasColumnName("archive_children_transitioned_at");
        builder.Property(x => x.ArchiveCleanupEligibleAt).HasColumnName("archive_cleanup_eligible_at");
        builder.Property(x => x.ArchiveCleanupCompletedAt).HasColumnName("archive_cleanup_completed_at");
    }
}
