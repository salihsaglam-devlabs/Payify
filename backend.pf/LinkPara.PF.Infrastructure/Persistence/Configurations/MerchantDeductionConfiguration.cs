using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class MerchantDeductionConfiguration : IEntityTypeConfiguration<MerchantDeduction>
{
    public void Configure(EntityTypeBuilder<MerchantDeduction> builder)
    {
        builder.Property(b => b.MerchantId).IsRequired();
        builder.Property(b => b.MerchantTransactionId).IsRequired();
        builder.Property(b => b.TotalDeductionAmount).HasPrecision(18, 4);
        builder.Property(b => b.RemainingDeductionAmount).HasPrecision(18, 4);
        builder.Property(b => b.DeductionAmountWithCommission).HasPrecision(18, 4);
        builder.Property(b => b.DeductionType).IsRequired();
        builder.Property(b => b.DeductionStatus).IsRequired();
        builder.Property(b => b.ExecutionDate).HasColumnType("Date").IsRequired();
        builder.Property(b => b.Currency).IsRequired();
        builder.Property(b => b.ConversationId).HasMaxLength(50);
    }
}