using LinkPara.Card.Domain.Entities.Reconciliation.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.Reconciliation;

public class ReconciliationOperationConfiguration : IEntityTypeConfiguration<ReconciliationOperation>
{
    public void Configure(EntityTypeBuilder<ReconciliationOperation> builder)
    {
        ConfigureColumns(builder);

        builder.HasIndex(x => new { x.GroupId, x.EvaluationId, x.SequenceIndex }).IsUnique().HasDatabaseName("ix_operation_group_id_evaluation_id_sequence_index");
        builder.HasIndex(x => x.FileLineId);
        builder.HasIndex(x => x.EvaluationId);
        builder.HasIndex(x => new { x.EvaluationId, x.SequenceIndex });
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
        builder.Property(x => x.FileLineId).HasColumnName("file_line_id");
        builder.Property(x => x.EvaluationId).HasColumnName("evaluation_id");
        builder.Property(x => x.GroupId).HasColumnName("group_id");
        builder.Property(x => x.SequenceIndex).HasColumnName("sequence_number");
        builder.Property(x => x.ParentSequenceIndex).HasColumnName("parent_sequence_number");
        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(200);
        builder.Property(x => x.Note).HasColumnName("note").HasMaxLength(2000);
        builder.Property(x => x.Payload).HasColumnName("payload");
        builder.Property(x => x.IsManual).HasColumnName("is_manual");
        builder.Property(x => x.Branch).HasColumnName("branch").HasMaxLength(50);
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.LeaseOwner).HasColumnName("lease_owner").HasMaxLength(200);
        builder.Property(x => x.LeaseExpiresAt).HasColumnName("lease_expires_at");
        builder.Property(x => x.RetryCount).HasColumnName("retry_count");
        builder.Property(x => x.MaxRetries).HasColumnName("max_retry_count");
        builder.Property(x => x.NextAttemptAt).HasColumnName("next_attempt_at");
        builder.Property(x => x.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(200);
        builder.Property(x => x.LastError).HasColumnName("last_error").HasMaxLength(2000);
    }
}
