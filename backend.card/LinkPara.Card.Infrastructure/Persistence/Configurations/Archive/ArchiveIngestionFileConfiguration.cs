using LinkPara.Card.Domain.Entities.Archive;
using LinkPara.Card.Infrastructure.Persistence.Configurations.FileIngestion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.Archive;

public class ArchiveIngestionFileConfiguration : IEntityTypeConfiguration<ArchiveIngestionFile>
{
    public void Configure(EntityTypeBuilder<ArchiveIngestionFile> builder)
    {
        builder.HasBaseType((Type?)null);

        IngestionFileConfiguration.ConfigureColumns(builder);

        builder.Property(x => x.ArchivedAt).HasColumnName("archived_at");
    }
}

