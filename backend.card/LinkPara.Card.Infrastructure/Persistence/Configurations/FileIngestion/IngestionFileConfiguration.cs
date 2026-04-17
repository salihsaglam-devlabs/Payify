using LinkPara.Card.Domain.Entities.FileIngestion.Persistence;
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
        builder.Property(x => x.FileKey).HasMaxLength(128).IsRequired();
        builder.Property(x => x.FileName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.FilePath).HasMaxLength(1024).IsRequired();
        builder.Property(x => x.SourceType).HasConversion<string>().IsRequired();
        builder.Property(x => x.FileType).HasConversion<string>().IsRequired();
        builder.Property(x => x.ContentType).HasConversion<string>().IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().IsRequired();
        builder.Property(x => x.Message).HasMaxLength(4000);
        builder.Property(x => x.ExpectedLineCount);
        builder.Property(x => x.ProcessedLineCount);
        builder.Property(x => x.SuccessfulLineCount);
        builder.Property(x => x.FailedLineCount);
        builder.Property(x => x.LastProcessedLineNumber);
        builder.Property(x => x.LastProcessedByteOffset);
        builder.Property(x => x.IsArchived);
    }
}
