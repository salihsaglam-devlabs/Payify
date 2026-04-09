using LinkPara.Card.Domain.Entities.Archive;
using LinkPara.Card.Infrastructure.Persistence.Configurations.Reconciliation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.Archive;

public class ArchiveReconciliationReviewConfiguration : IEntityTypeConfiguration<ArchiveReconciliationReview>
{
    public void Configure(EntityTypeBuilder<ArchiveReconciliationReview> builder)
    {
        builder.HasBaseType((Type?)null);

        ReconciliationReviewConfiguration.ConfigureColumns(builder);

        builder.Property(x => x.ArchivedAt).HasColumnName("archived_at");
        builder.Property(x => x.ArchivedBy).HasColumnName("archived_by").HasMaxLength(100);
        builder.Property(x => x.ArchiveRunId).HasColumnName("archive_run_id");

        builder.Ignore(x => x.IngestionFileLine);
        builder.Ignore(x => x.Evaluation);
        builder.Ignore(x => x.Operation);
    }
}

