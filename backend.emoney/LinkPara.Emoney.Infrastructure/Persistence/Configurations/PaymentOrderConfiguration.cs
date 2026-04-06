using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class PaymentOrderConfiguration : IEntityTypeConfiguration<PaymentOrder>
{
    public void Configure(EntityTypeBuilder<PaymentOrder> builder)
    {
        builder.Property(p => p.ConsentNumber).HasMaxLength(50);
        builder.Property(p => p.YosCode).HasMaxLength(50);
        builder.Property(p => p.Amount).HasPrecision(18, 2);
        builder.Property(p => p.CurrencyCode).HasMaxLength(10);
        builder.Property(p => p.SenderTitle).HasMaxLength(300);
        builder.Property(p => p.SenderWalletNumber).HasMaxLength(50);
        builder.Property(p => p.ReceiverTitle).HasMaxLength(300);
        builder.Property(p => p.ReceiverWalletNumber).HasMaxLength(50);
        builder.Property(p => p.ReceiverIban).HasMaxLength(50);
    }
}
