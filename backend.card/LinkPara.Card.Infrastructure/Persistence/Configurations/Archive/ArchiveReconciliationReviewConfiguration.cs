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


        builder.Ignore(x => x.IngestionFileLine);
        builder.Ignore(x => x.Evaluation);
        builder.Ignore(x => x.Operation);
    }
}

