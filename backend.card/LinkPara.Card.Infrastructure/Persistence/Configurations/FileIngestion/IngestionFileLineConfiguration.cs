using LinkPara.Card.Domain.Entities.FileIngestion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.FileIngestion;

public class IngestionFileLineConfiguration : IEntityTypeConfiguration<IngestionFileLine>
{
    public void Configure(EntityTypeBuilder<IngestionFileLine> builder)
    {
        ConfigureColumns(builder);

        builder.HasIndex(x => new { x.IngestionFileId, x.LineNumber }).IsUnique();
        builder.HasIndex(x => new { x.IngestionFileId, x.Status });
        builder.HasIndex(x => new { x.IngestionFileId, x.RecordType, x.Status });
        builder.HasIndex(x => new { x.IngestionFileId, x.ByteOffset });
        builder.HasIndex(x => new { x.IngestionFileId, x.DuplicateDetectionKey });
        builder.HasIndex(x => new { x.IngestionFileId, x.DuplicateStatus });
        builder.HasIndex(x => x.DuplicateGroupId);
        builder.HasIndex(x => x.CorrelationKey);
        builder.HasIndex(x => new { x.CorrelationKey, x.CorrelationValue });
        builder.HasIndex(x => x.ReconciliationStatus);
        builder.HasIndex(x => new { x.IngestionFileId, x.RecordType, x.ReconciliationStatus, x.UpdateDate });
        builder.HasOne(x => x.IngestionFile)
            .WithMany()
            .HasForeignKey(x => x.IngestionFileId);
    }

    internal static void ConfigureColumns<T>(EntityTypeBuilder<T> builder) where T : IngestionFileLine
    {
        builder.Property(x => x.IngestionFileId).HasColumnName("file_id");
        builder.Property(x => x.LineNumber).HasColumnName("line_number");
        builder.Property(x => x.ByteOffset).HasColumnName("byte_offset");
        builder.Property(x => x.ByteLength).HasColumnName("byte_length");
        builder.Property(x => x.RecordType).HasColumnName("line_type").HasMaxLength(8);
        builder.Property(x => x.RawData).HasColumnName("raw_content");
        builder.Property(x => x.ParsedData).HasColumnName("parsed_content");
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Message).HasColumnName("message").HasMaxLength(4000);
        builder.Property(x => x.RetryCount).HasColumnName("retry_count");
        builder.Property(x => x.CorrelationKey).HasColumnName("correlation_key").HasMaxLength(256);
        builder.Property(x => x.CorrelationValue).HasColumnName("correlation_value").HasMaxLength(1024);
        builder.Property(x => x.DuplicateDetectionKey).HasColumnName("duplicate_detection_key").HasMaxLength(256);
        builder.Property(x => x.DuplicateStatus).HasColumnName("duplicate_status").HasMaxLength(64);
        builder.Property(x => x.DuplicateGroupId).HasColumnName("duplicate_group_id");
        builder.Property(x => x.ReconciliationStatus).HasColumnName("reconciliation_status").HasConversion<string>().HasMaxLength(32);
    }
}
