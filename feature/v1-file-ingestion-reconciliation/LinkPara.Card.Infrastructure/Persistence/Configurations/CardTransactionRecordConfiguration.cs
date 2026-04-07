using LinkPara.Card.Domain.Entities.FileIngestion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations;

public class CardTransactionRecordConfiguration : IEntityTypeConfiguration<CardTransactionRecord>
{
    public void Configure(EntityTypeBuilder<CardTransactionRecord> builder)
    {
        builder.Property(x => x.OceanTxnGuid).HasMaxLength(32);
        builder.Property(x => x.OceanMainTxnGuid).HasMaxLength(32);
        builder.Property(x => x.CardNo).HasMaxLength(32);
        builder.Property(x => x.Rrn).HasMaxLength(24);
        builder.Property(x => x.Arn).HasMaxLength(32);
        builder.Property(x => x.ProvisionCode).HasMaxLength(16);
        builder.Property(x => x.Otc).HasMaxLength(8);
        builder.Property(x => x.Ots).HasMaxLength(8);
        builder.Property(x => x.CorrelationKey).HasMaxLength(256);
        builder.Property(x => x.ReconciliationState)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();
        builder.Property(x => x.LastReconciliationExecutionGroupId).HasColumnName("last_reconciliation_execution_group_id");
        builder.Property(x => x.ReconciliationStateReason).HasMaxLength(120);

        builder.HasIndex(x => x.OceanTxnGuid);
        builder.HasIndex(x => new { x.CardNo, x.Otc, x.Ots, x.CardHolderBillingAmount });
        builder.HasIndex(x => x.CorrelationKey);
        builder.HasIndex(x => new { x.ReconciliationState, x.ReconciliationStateUpdatedAt });
        builder.HasIndex(x => x.LastReconciliationExecutionGroupId);

        builder
            .HasOne(x => x.ImportedFileRow)
            .WithOne(x => x.CardTransactionRecord)
            .HasForeignKey<CardTransactionRecord>(x => x.ImportedFileRowId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
