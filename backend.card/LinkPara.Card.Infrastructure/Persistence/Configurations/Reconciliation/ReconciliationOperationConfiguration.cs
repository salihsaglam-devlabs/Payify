using LinkPara.Card.Domain.Entities.Reconciliation.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.Reconciliation;

public class ReconciliationOperationConfiguration : IEntityTypeConfiguration<ReconciliationOperation>
{
    public void Configure(EntityTypeBuilder<ReconciliationOperation> builder)
    {
        ConfigureColumns(builder);

        builder.HasIndex(x => new { x.GroupId, x.EvaluationId, x.SequenceNumber }).IsUnique().HasDatabaseName("ix_operation_group_id_evaluation_id_sequence_index");
        builder.HasIndex(x => x.FileLineId);
        builder.HasIndex(x => x.EvaluationId);
        builder.HasIndex(x => new { x.EvaluationId, x.SequenceNumber });
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => new { x.Status, x.NextAttemptAt, x.LeaseExpiresAt });
        builder.HasIndex(x => x.LeaseOwner);
        builder.HasIndex(x => x.IdempotencyKey);

        builder.HasOne(x => x.IngestionFileLine)
            .WithMany()
            .HasForeignKey(x => x.FileLineId);

        builder.HasOne(x => x.Evaluation)
            .WithMany()
            .HasForeignKey(x => x.EvaluationId);
    }

    internal static void ConfigureColumns<T>(EntityTypeBuilder<T> builder) where T : ReconciliationOperation
    {
        builder.Property(x => x.FileLineId);
        builder.Property(x => x.EvaluationId);
        builder.Property(x => x.GroupId);
        builder.Property(x => x.SequenceNumber);
        builder.Property(x => x.ParentSequenceNumber);
        builder.Property(x => x.Code).HasMaxLength(200);
        builder.Property(x => x.Note).HasMaxLength(2000);
        builder.Property(x => x.Payload);
        builder.Property(x => x.IsManual);
        builder.Property(x => x.Branch).HasMaxLength(50);
        builder.Property(x => x.Status).HasConversion<string>().IsRequired();
        builder.Property(x => x.LeaseOwner).HasMaxLength(200);
        builder.Property(x => x.LeaseExpiresAt);
        builder.Property(x => x.RetryCount);
        builder.Property(x => x.MaxRetries);
        builder.Property(x => x.NextAttemptAt);
        builder.Property(x => x.IdempotencyKey).HasMaxLength(200);
        builder.Property(x => x.LastError).HasMaxLength(2000);
    }
}
