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
        builder.Property(x => x.ArchivedBy).HasColumnName("archived_by").HasMaxLength(100);
        builder.Property(x => x.ArchiveRunId).HasColumnName("archive_run_id");
        builder.Property(x => x.ArchiveRecordWrittenAt).HasColumnName("archive_record_written_at");
        builder.Property(x => x.ArchiveRecordRunId).HasColumnName("archive_record_run_id");
        builder.Property(x => x.ArchiveChildrenTransitionedAt).HasColumnName("archive_children_transitioned_at");
        builder.Property(x => x.ArchiveCleanupEligibleAt).HasColumnName("archive_cleanup_eligible_at");
        builder.Property(x => x.ArchiveCleanupCompletedAt).HasColumnName("archive_cleanup_completed_at");
    }
}

