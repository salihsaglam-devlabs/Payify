using LinkPara.Card.Domain.Entities.FileIngestion.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.FileIngestion;

public class IngestionFileLineConfiguration : IEntityTypeConfiguration<IngestionFileLine>
{
    public void Configure(EntityTypeBuilder<IngestionFileLine> builder)
    {
        ConfigureColumns(builder);

        builder.HasIndex(x => new { x.FileId, x.LineNumber }).IsUnique();
        builder.HasIndex(x => new { x.FileId, x.Status });
        builder.HasIndex(x => new { x.FileId, x.LineType, x.Status });
        builder.HasIndex(x => new { x.FileId, x.ByteOffset });
        builder.HasIndex(x => new { x.FileId, x.DuplicateDetectionKey });
        builder.HasIndex(x => new { x.FileId, x.DuplicateStatus });
        builder.HasIndex(x => x.DuplicateGroupId);
        builder.HasIndex(x => x.CorrelationKey);
        builder.HasIndex(x => new { x.CorrelationKey, x.CorrelationValue });
        builder.HasIndex(x => x.ReconciliationStatus);
        builder.HasIndex(x => new { x.FileId, x.LineType, x.ReconciliationStatus, x.UpdateDate });
        builder.HasIndex(x => x.MatchedClearingLineId);
        builder.HasOne(x => x.IngestionFile)
            .WithMany()
            .HasForeignKey(x => x.FileId);
    }

    internal static void ConfigureColumns<T>(EntityTypeBuilder<T> builder) where T : IngestionFileLine
    {
        builder.Property(x => x.FileId);
        builder.Property(x => x.LineNumber);
        builder.Property(x => x.ByteOffset);
        builder.Property(x => x.ByteLength);
        builder.Property(x => x.LineType).HasMaxLength(8);
        builder.Property(x => x.RawContent);
        builder.Property(x => x.ParsedContent);
        builder.Property(x => x.Status).HasConversion<string>().IsRequired();
        builder.Property(x => x.Message).HasMaxLength(4000);
        builder.Property(x => x.RetryCount);
        builder.Property(x => x.CorrelationKey).HasMaxLength(256);
        builder.Property(x => x.CorrelationValue).HasMaxLength(1024);
        builder.Property(x => x.DuplicateDetectionKey).HasMaxLength(256);
        builder.Property(x => x.DuplicateStatus).HasMaxLength(64);
        builder.Property(x => x.DuplicateGroupId);
        builder.Property(x => x.ReconciliationStatus).HasConversion<string>();
        builder.Property(x => x.MatchedClearingLineId);
    }
}
