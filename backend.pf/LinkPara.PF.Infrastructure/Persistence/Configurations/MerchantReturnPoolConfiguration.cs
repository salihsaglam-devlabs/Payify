using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class MerchantReturnPoolConfiguration : IEntityTypeConfiguration<MerchantReturnPool>
{
    public void Configure(EntityTypeBuilder<MerchantReturnPool> builder)
    {
        builder.Property(b => b.OrderId).IsRequired().HasMaxLength(50);
        builder.Property(b => b.ConversationId).IsRequired().HasMaxLength(50);
        builder.Property(b => b.ClientIpAddress).IsRequired().HasMaxLength(50);
        builder.Property(b => b.Amount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.BankCode).IsRequired();
        builder.Property(b => b.BankName).HasMaxLength(100);
        builder.Property(b => b.RejectDescription).HasMaxLength(400);
        builder.Property(b => b.RejectReason).HasMaxLength(400);
        builder.Property(b => b.CardNumber).HasMaxLength(50);
        builder.Property(b => b.CurrencyCode).HasMaxLength(10);
        builder.Property(b => b.BankResponseCode).HasMaxLength(50);
        builder.Property(b => b.BankResponseDescription).HasMaxLength(1000);
    }
}
