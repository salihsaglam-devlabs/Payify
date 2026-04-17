using LinkPara.Card.Domain.Entities.Archive.Persistence;
using LinkPara.Card.Infrastructure.Persistence.Configurations.FileIngestion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.Archive;

public class ArchiveIngestionClearingMscDetailConfiguration : IEntityTypeConfiguration<ArchiveIngestionClearingMscDetail>
{
    public void Configure(EntityTypeBuilder<ArchiveIngestionClearingMscDetail> builder)
    {
        builder.HasBaseType((Type?)null);

        IngestionClearingMscDetailConfiguration.ConfigureColumns(builder);

        builder.Ignore(x => x.FileLine);
    }
}

