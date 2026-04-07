using LinkPara.Card.Domain.Entities.FileIngestion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.FileIngestion;

public class IngestionFileConfiguration : IEntityTypeConfiguration<IngestionFile>
{
    public void Configure(EntityTypeBuilder<IngestionFile> builder)
    {
        builder.Property(x => x.FileKey).HasColumnName("file_key");
        builder.Property(x => x.FileName).HasColumnName("file_name");
        builder.Property(x => x.FullPath).HasColumnName("file_path");
        builder.Property(x => x.SourceType).HasColumnName("source_type");
        builder.Property(x => x.FileType).HasColumnName("file_type");
        builder.Property(x => x.ContentType).HasColumnName("content_type");
        builder.Property(x => x.Status).HasColumnName("status");
        builder.Property(x => x.Message).HasColumnName("message");
        builder.Property(x => x.ExpectedCount).HasColumnName("expected_line_count");
        builder.Property(x => x.TotalCount).HasColumnName("processed_line_count");
        builder.Property(x => x.SuccessCount).HasColumnName("successful_line_count");
        builder.Property(x => x.ErrorCount).HasColumnName("failed_line_count");
        builder.Property(x => x.LastProcessedLineNumber).HasColumnName("last_processed_line_number");
        builder.Property(x => x.LastProcessedByteOffset).HasColumnName("last_processed_byte_offset");
        builder.Property(x => x.IsArchived).HasColumnName("is_archived");

        builder.Property(x => x.FileKey).HasMaxLength(128).IsRequired();
        builder.Property(x => x.FileName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.FullPath).HasMaxLength(1024).IsRequired();
        builder.Property(x => x.SourceType).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.FileType).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.ContentType).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Message).HasMaxLength(4000);
        builder.HasIndex(x => new { x.FileKey, x.FileName, x.SourceType, x.FileType })
            .IsUnique();
        builder.HasIndex(x => x.Status);
    }
}
