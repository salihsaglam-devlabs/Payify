using LinkPara.Card.Domain.Entities.Archive.Persistence;
using LinkPara.Card.Infrastructure.Persistence.Configurations.FileIngestion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.Archive;

public class ArchiveIngestionCardMscDetailConfiguration : IEntityTypeConfiguration<ArchiveIngestionCardMscDetail>
{
    public void Configure(EntityTypeBuilder<ArchiveIngestionCardMscDetail> builder)
    {
        builder.HasBaseType((Type?)null);

        IngestionCardMscDetailConfiguration.ConfigureColumns(builder);

        builder.Ignore(x => x.IngestionFileLine);
    }
}

