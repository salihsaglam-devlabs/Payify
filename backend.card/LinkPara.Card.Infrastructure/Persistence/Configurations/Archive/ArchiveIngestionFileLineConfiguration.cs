using LinkPara.Card.Domain.Entities.Archive.Persistence;
using LinkPara.Card.Infrastructure.Persistence.Configurations.FileIngestion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.Archive;

public class ArchiveIngestionFileLineConfiguration : IEntityTypeConfiguration<ArchiveIngestionFileLine>
{
    public void Configure(EntityTypeBuilder<ArchiveIngestionFileLine> builder)
    {
        builder.HasBaseType((Type?)null);

        IngestionFileLineConfiguration.ConfigureColumns(builder);


        builder.Ignore(x => x.IngestionFile);
    }
}

