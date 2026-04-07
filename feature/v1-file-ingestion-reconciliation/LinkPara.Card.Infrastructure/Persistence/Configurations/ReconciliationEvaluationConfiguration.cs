using LinkPara.Card.Domain.Entities.FileIngestion;
using LinkPara.Card.Domain.Entities.Reconciliation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations;

public class ReconciliationEvaluationConfiguration : IEntityTypeConfiguration<ReconciliationEvaluation>
{
    public void Configure(EntityTypeBuilder<ReconciliationEvaluation> builder)
    {
        builder.Property(x => x.CardTransactionRecordId).HasColumnName("card_transaction_record_id").IsRequired();
        builder.Property(x => x.RunId).HasColumnName("run_id");
        builder.Property(x => x.ExecutionGroupId).HasColumnName("execution_group_id").IsRequired();
        builder.Property(x => x.DecisionType).HasColumnName("decision_type").HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(x => x.DecisionCode).HasColumnName("decision_code").IsRequired().HasMaxLength(80);
        builder.Property(x => x.DecisionReason).HasColumnName("decision_reason").HasMaxLength(500);
        builder.Property(x => x.HasClearing).HasColumnName("has_clearing").IsRequired();
        builder.Property(x => x.ClearingRecordId).HasColumnName("clearing_record_id");
        builder.Property(x => x.PlannedOperationCount).HasColumnName("planned_operation_count").IsRequired();
        builder.Property(x => x.PlannedOperationCodes).HasColumnName("planned_operation_codes");

        builder.HasIndex(x => x.CardTransactionRecordId);
        builder.HasIndex(x => x.ExecutionGroupId);
        builder.HasIndex(x => new { x.DecisionType, x.CreateDate });

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
