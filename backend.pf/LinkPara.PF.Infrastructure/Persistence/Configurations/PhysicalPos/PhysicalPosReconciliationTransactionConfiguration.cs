using LinkPara.PF.Domain.Entities.PhysicalPos;
using LinkPara.PF.Domain.Enums.PhysicalPos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations.PhysicalPos;

public class PhysicalPosReconciliationTransactionConfiguration : IEntityTypeConfiguration<PhysicalPosReconciliationTransaction>
{
    public void Configure(EntityTypeBuilder<PhysicalPosReconciliationTransaction> builder)
    {
        builder.Property(b => b.PaymentId).IsRequired().HasMaxLength(50);
        builder.Property(b => b.BatchId).IsRequired().HasMaxLength(50);
        builder.Property(b => b.Type).IsRequired().HasMaxLength(50);
        builder.Property(b => b.Status).IsRequired().HasMaxLength(20);
        builder.Property(b => b.Currency).IsRequired().HasMaxLength(10);
        builder.Property(b => b.MerchantId).IsRequired().HasMaxLength(50);
        builder.Property(b => b.TerminalId).IsRequired().HasMaxLength(50);
        builder.Property(b => b.Amount).HasPrecision(18, 4);
        builder.Property(b => b.PointAmount).HasPrecision(18, 4);
        builder.Property(b => b.MaskedCardNo).IsRequired().HasMaxLength(50);
        builder.Property(b => b.BinNumber).IsRequired().HasMaxLength(10);
        builder.Property(b => b.ProvisionNo).HasMaxLength(50);
        builder.Property(b => b.AcquirerResponseCode).HasMaxLength(10);
        builder.Property(b => b.Vendor).HasMaxLength(20);
        builder.Property(b => b.Rrn).HasMaxLength(50);
        builder.Property(b => b.Stan).HasMaxLength(50);
        builder.Property(b => b.PosEntryMode).HasMaxLength(20);
        builder.Property(b => b.PinEntryInfo).HasMaxLength(20);
        builder.Property(b => b.BankRef).HasMaxLength(50);
        builder.Property(b => b.OriginalRef).HasMaxLength(50);
        builder.Property(b => b.ConversationId).IsRequired().HasMaxLength(50);
        builder.Property(b => b.ClientIpAddress).HasMaxLength(50);
        builder.Property(b => b.SerialNumber).IsRequired().HasMaxLength(200);
        builder.Property(b => b.ReconciliationStatus).IsRequired().HasDefaultValue(ReconciliationStatus.Pending);
        builder.Property(b => b.ErrorCode).HasMaxLength(20);
        builder.Property(b => b.ErrorMessage).HasMaxLength(300);
    }
}
