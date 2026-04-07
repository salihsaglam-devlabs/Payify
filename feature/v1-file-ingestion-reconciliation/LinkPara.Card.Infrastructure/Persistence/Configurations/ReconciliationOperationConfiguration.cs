using LinkPara.Card.Domain.Entities.FileIngestion;
using LinkPara.Card.Domain.Entities.Reconciliation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations;

public class ReconciliationOperationConfiguration : IEntityTypeConfiguration<ReconciliationOperation>
{
    public void Configure(EntityTypeBuilder<ReconciliationOperation> builder)
    {
        builder.Property(x => x.CardTransactionRecordId).HasColumnName("card_transaction_record_id").IsRequired();
        builder.Property(x => x.ClearingRecordId).HasColumnName("clearing_record_id");
        builder.Property(x => x.RunId).HasColumnName("run_id").IsRequired();
        builder.Property(x => x.ExecutionGroupId).HasColumnName("execution_group_id").IsRequired();
        builder.Property(x => x.OperationIndex).HasColumnName("operation_index").IsRequired();
        builder.Property(x => x.DependsOnIndex).HasColumnName("depends_on_index");
        builder.Property(x => x.IsApprovalGate).HasColumnName("is_approval_gate").HasDefaultValue(false);
        builder.Property(x => x.OperationCode).HasColumnName("operation_code").IsRequired().HasMaxLength(80);
        builder.Property(x => x.Mode).HasColumnName("mode").HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.Fingerprint).HasColumnName("fingerprint").HasMaxLength(200);
        builder.Property(x => x.IdempotencyKey).HasColumnName("idempotency_key").IsRequired().HasMaxLength(300);
        builder.Property(x => x.StartedAt).HasColumnName("started_at");
        builder.Property(x => x.CompletedAt).HasColumnName("completed_at");
        builder.Property(x => x.ErrorCode).HasColumnName("error_code").HasMaxLength(100);
        builder.Property(x => x.ErrorText).HasColumnName("error_text").HasMaxLength(2000);
        builder.Property(x => x.Payload).HasColumnName("payload");

        builder.HasIndex(x => new { x.CardTransactionRecordId, x.RunId });
        builder.HasIndex(x => new { x.CardTransactionRecordId, x.CreateDate });
        builder.HasIndex(x => x.ExecutionGroupId);
        builder.HasIndex(x => new { x.Mode, x.Status, x.CreateDate });
        builder.HasIndex(x => new { x.RunId, x.OperationIndex }).IsUnique();
        builder.HasIndex(x => x.IdempotencyKey).IsUnique();
        builder.HasOne<CardTransactionRecord>()
            .WithMany()
            .HasForeignKey(x => x.CardTransactionRecordId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ClearingRecord>()
            .WithMany()
            .HasForeignKey(x => x.ClearingRecordId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
