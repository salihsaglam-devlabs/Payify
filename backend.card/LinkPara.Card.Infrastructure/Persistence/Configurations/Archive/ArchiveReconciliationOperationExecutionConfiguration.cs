using LinkPara.Card.Domain.Entities.Archive;
using LinkPara.Card.Infrastructure.Persistence.Configurations.Reconciliation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.Archive;

public class ArchiveReconciliationOperationExecutionConfiguration : IEntityTypeConfiguration<ArchiveReconciliationOperationExecution>
{
    public void Configure(EntityTypeBuilder<ArchiveReconciliationOperationExecution> builder)
    {
        builder.HasBaseType((Type?)null);

        ReconciliationOperationExecutionConfiguration.ConfigureColumns(builder);


        builder.Ignore(x => x.IngestionFileLine);
        builder.Ignore(x => x.Evaluation);
        builder.Ignore(x => x.Operation);
    }
}

