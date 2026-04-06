using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class DeductionTransactionConfiguration : IEntityTypeConfiguration<DeductionTransaction>
{
    public void Configure(EntityTypeBuilder<DeductionTransaction> builder)
    {
        builder.Property(b => b.Amount).HasPrecision(18, 4);
        builder.Property(b => b.DeductionType).IsRequired();
        builder.Property(b => b.MerchantId).IsRequired();
        builder.Property(b => b.PostingBalanceId).IsRequired();
        builder.Property(b => b.MerchantDeductionId).IsRequired();
    }
}