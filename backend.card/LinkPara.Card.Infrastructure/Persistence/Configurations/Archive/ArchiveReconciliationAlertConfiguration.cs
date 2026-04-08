using LinkPara.Card.Domain.Entities.Archive;
using LinkPara.Card.Infrastructure.Persistence.Configurations.Reconciliation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.Archive;

public class ArchiveReconciliationAlertConfiguration : IEntityTypeConfiguration<ArchiveReconciliationAlert>
{
    public void Configure(EntityTypeBuilder<ArchiveReconciliationAlert> builder)
    {
        builder.HasBaseType((Type?)null);

        ReconciliationAlertConfiguration.ConfigureColumns(builder);

        builder.Property(x => x.ArchivedAt).HasColumnName("archived_at");

        builder.Ignore(x => x.IngestionFileLine);
        builder.Ignore(x => x.Evaluation);
        builder.Ignore(x => x.Operation);
    }
}

