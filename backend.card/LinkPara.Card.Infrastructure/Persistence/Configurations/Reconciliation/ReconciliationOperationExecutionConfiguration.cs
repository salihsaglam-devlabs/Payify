using LinkPara.Card.Domain.Entities.Reconciliation.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.Reconciliation;

public class ReconciliationOperationExecutionConfiguration : IEntityTypeConfiguration<ReconciliationOperationExecution>
{
    public void Configure(EntityTypeBuilder<ReconciliationOperationExecution> builder)
    {
        ConfigureColumns(builder);

        builder.HasIndex(x => new { x.OperationId, x.AttemptNumber }).IsUnique(false);
        builder.HasIndex(x => x.OperationId);
        builder.HasIndex(x => x.FileLineId);
        builder.HasIndex(x => x.GroupId);
        builder.HasIndex(x => x.EvaluationId);
        builder.HasIndex(x => new { x.EvaluationId, x.OperationId });

        builder.HasOne(x => x.IngestionFileLine)
            .WithMany()
            .HasForeignKey(x => x.FileLineId);
        builder.HasOne(x => x.Evaluation)
            .WithMany()
            .HasForeignKey(x => x.EvaluationId);
        builder.HasOne(x => x.Operation)
            .WithMany()
            .HasForeignKey(x => x.OperationId);
    }

    internal static void ConfigureColumns<T>(EntityTypeBuilder<T> builder) where T : ReconciliationOperationExecution
    {
        builder.Property(x => x.FileLineId);
        builder.Property(x => x.GroupId);
        builder.Property(x => x.EvaluationId);
        builder.Property(x => x.OperationId);
        builder.Property(x => x.AttemptNumber);
        builder.Property(x => x.StartedAt);
        builder.Property(x => x.FinishedAt);
        builder.Property(x => x.Status).HasConversion<string>().IsRequired();
        builder.Property(x => x.RequestPayload);
        builder.Property(x => x.ResponsePayload);
        builder.Property(x => x.ResultCode).HasMaxLength(100);
        builder.Property(x => x.ResultMessage).HasMaxLength(2000);
        builder.Property(x => x.ErrorCode).HasMaxLength(100);
        builder.Property(x => x.ErrorMessage).HasMaxLength(4000);
    }
}
