using LinkPara.Card.Domain.Entities.Archive.Persistence;
using LinkPara.Card.Infrastructure.Persistence.Configurations.Reconciliation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.Archive;

public class ArchiveReconciliationOperationConfiguration : IEntityTypeConfiguration<ArchiveReconciliationOperation>
{
    public void Configure(EntityTypeBuilder<ArchiveReconciliationOperation> builder)
    {
        builder.HasBaseType((Type?)null);

        ReconciliationOperationConfiguration.ConfigureColumns(builder);


        builder.Ignore(x => x.IngestionFileLine);
        builder.Ignore(x => x.Evaluation);
    }
}

