using LinkPara.Card.Domain.Entities.Reconciliation.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.Reconciliation;

public class ReconciliationEvaluationConfiguration : IEntityTypeConfiguration<ReconciliationEvaluation>
{
    public void Configure(EntityTypeBuilder<ReconciliationEvaluation> builder)
    {
        ConfigureColumns(builder);

        builder.HasIndex(x => x.FileLineId);
        builder.HasIndex(x => x.GroupId);
        builder.HasOne(x => x.IngestionFileLine)
            .WithMany()
            .HasForeignKey(x => x.FileLineId);
    }

    internal static void ConfigureColumns<T>(EntityTypeBuilder<T> builder) where T : ReconciliationEvaluation
    {
        builder.Property(x => x.FileLineId);
        builder.Property(x => x.GroupId);
        builder.Property(x => x.Status).HasConversion<string>().IsRequired();
        builder.Property(x => x.Message).HasMaxLength(1000);
        builder.Property(x => x.OperationCount);
    }
}
