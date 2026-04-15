using LinkPara.Card.Domain.Entities.Archive;
using LinkPara.Card.Infrastructure.Persistence.Configurations.FileIngestion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.Archive;

public class ArchiveIngestionClearingBkmDetailConfiguration : IEntityTypeConfiguration<ArchiveIngestionClearingBkmDetail>
{
    public void Configure(EntityTypeBuilder<ArchiveIngestionClearingBkmDetail> builder)
    {
        builder.HasBaseType((Type?)null);

        IngestionClearingBkmDetailConfiguration.ConfigureColumns(builder);

        builder.Ignore(x => x.IngestionFileLine);
    }
}

