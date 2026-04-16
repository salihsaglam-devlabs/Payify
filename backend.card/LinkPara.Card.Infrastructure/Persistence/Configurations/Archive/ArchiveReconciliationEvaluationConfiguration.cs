using LinkPara.Card.Domain.Entities.Archive.Persistence;
using LinkPara.Card.Infrastructure.Persistence.Configurations.Reconciliation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.Archive;

public class ArchiveReconciliationEvaluationConfiguration : IEntityTypeConfiguration<ArchiveReconciliationEvaluation>
{
    public void Configure(EntityTypeBuilder<ArchiveReconciliationEvaluation> builder)
    {
        builder.HasBaseType((Type?)null);

        ReconciliationEvaluationConfiguration.ConfigureColumns(builder);


        builder.Ignore(x => x.IngestionFileLine);
    }
}

