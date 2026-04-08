using LinkPara.Card.Domain.Entities.Archive;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.Archive;

public class ArchiveReconciliationOperationConfiguration : IEntityTypeConfiguration<ArchiveReconciliationOperation>
{
    public void Configure(EntityTypeBuilder<ArchiveReconciliationOperation> builder)
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
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
        builder.Property(x => x.LeaseOwner).HasColumnName("lease_owner").HasMaxLength(200);
        builder.Property(x => x.LeaseExpiresAt).HasColumnName("lease_expires_at");
        builder.Property(x => x.RetryCount).HasColumnName("retry_count");
        builder.Property(x => x.MaxRetries).HasColumnName("max_retry_count");
        builder.Property(x => x.NextAttemptAt).HasColumnName("next_attempt_at");
        builder.Property(x => x.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(200);
        builder.Property(x => x.LastError).HasColumnName("last_error").HasMaxLength(2000);
        builder.Property(x => x.ArchiveRunId).HasColumnName("archive_run_id");
        builder.Property(x => x.ArchivedAt).HasColumnName("archived_at");
        builder.Property(x => x.ArchivedBy).HasColumnName("archived_by").HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.FileLineId);
        builder.HasIndex(x => x.EvaluationId);
        builder.HasIndex(x => x.ArchiveRunId);
    }
}
