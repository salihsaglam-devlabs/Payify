using LinkPara.Card.Domain.Entities.Archive.Persistence;
using LinkPara.Card.Infrastructure.Persistence.Configurations.FileIngestion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.Archive;

public class ArchiveIngestionCardBkmDetailConfiguration : IEntityTypeConfiguration<ArchiveIngestionCardBkmDetail>
{
    public void Configure(EntityTypeBuilder<ArchiveIngestionCardBkmDetail> builder)
    {
        builder.HasBaseType((Type?)null);

        IngestionCardBkmDetailConfiguration.ConfigureColumns(builder);

        builder.Ignore(x => x.IngestionFileLine);
    }
}

