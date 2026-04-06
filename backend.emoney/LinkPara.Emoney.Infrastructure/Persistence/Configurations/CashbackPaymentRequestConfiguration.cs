using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class CashbackPaymentRequestConfiguration : IEntityTypeConfiguration<CashbackPaymentRequest>
{
    public void Configure(EntityTypeBuilder<CashbackPaymentRequest> builder)
    {
        builder.Property(b => b.EntitlementId).IsRequired();
        builder.Property(b => b.CashbackPaymentStatus).IsRequired();
        builder.Property(b => b.WalletNumber).IsRequired().HasMaxLength(30);
        builder.Property(b => b.Amount).IsRequired().HasPrecision(18, 2);
        builder.Property(b => b.CurrencyCode).IsRequired().HasMaxLength(10);
        
    }
}

