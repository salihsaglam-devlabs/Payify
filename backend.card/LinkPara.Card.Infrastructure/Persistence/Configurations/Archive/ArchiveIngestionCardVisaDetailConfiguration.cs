using LinkPara.Card.Domain.Entities.Archive.Persistence;
using LinkPara.Card.Infrastructure.Persistence.Configurations.FileIngestion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.Archive;

public class ArchiveIngestionCardVisaDetailConfiguration : IEntityTypeConfiguration<ArchiveIngestionCardVisaDetail>
{
    public void Configure(EntityTypeBuilder<ArchiveIngestionCardVisaDetail> builder)
    {
        builder.HasBaseType((Type?)null);

        IngestionCardVisaDetailConfiguration.ConfigureColumns(builder);

        builder.Ignore(x => x.IngestionFileLine);
    }
}

