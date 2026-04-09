using LinkPara.Card.Domain.Entities.Archive;
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

        builder.Property(x => x.ArchivedAt).HasColumnName("archived_at");
        builder.Property(x => x.ArchivedBy).HasColumnName("archived_by").HasMaxLength(100);
        builder.Property(x => x.ArchiveRunId).HasColumnName("archive_run_id");

        builder.Ignore(x => x.IngestionFile);
    }
}

