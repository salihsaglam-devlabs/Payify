using LinkPara.Card.Domain.Entities.FileIngestion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations;

public class ImportedFileConfiguration : IEntityTypeConfiguration<ImportedFile>
{
    public void Configure(EntityTypeBuilder<ImportedFile> builder)
    {
        builder.Property(x => x.FileFamily).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.ProcessedAt).HasColumnName("processed_at");
        builder.Property(x => x.FileName).IsRequired().HasMaxLength(255);
        builder.Property(x => x.FileType).IsRequired().HasMaxLength(50);
        builder.Property(x => x.SourceType).IsRequired().HasMaxLength(20);
        builder.Property(x => x.SourcePath).HasMaxLength(500);
        builder.Property(x => x.FileHash).IsRequired().HasMaxLength(128);
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);

        builder.HasIndex(x => x.FileHash).IsUnique();
        builder.HasIndex(x => new { x.FileFamily, x.FileName, x.SourceType });
        builder.HasIndex(x => new { x.FileFamily, x.SourceType, x.DeclaredFileDate, x.DeclaredFileNo, x.DeclaredFileVersionNumber });
    }
}
