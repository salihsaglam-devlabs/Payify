using LinkPara.Card.Domain.Entities.Archive;
using LinkPara.Card.Infrastructure.Persistence.Configurations.FileIngestion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.Archive;

public class ArchiveIngestionClearingVisaDetailConfiguration : IEntityTypeConfiguration<ArchiveIngestionClearingVisaDetail>
{
    public void Configure(EntityTypeBuilder<ArchiveIngestionClearingVisaDetail> builder)
    {
        builder.HasBaseType((Type?)null);

        IngestionClearingVisaDetailConfiguration.ConfigureColumns(builder);

        builder.Ignore(x => x.IngestionFileLine);
    }
}

