using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class CardTopupRequestConfiguration : IEntityTypeConfiguration<CardTopupRequest>
{
    public void Configure(EntityTypeBuilder<CardTopupRequest> builder)
    {
        builder.Property(t => t.Amount).HasPrecision(18, 2);
        builder.Property(t => t.ConversationId).HasMaxLength(100);
        builder.Property(t => t.WalletNumber).HasMaxLength(50);
        builder.Property(t => t.CommissionTotal).HasPrecision(18, 2);
        builder.Property(t => t.BsmvTotal).HasPrecision(18, 2);
        builder.Property(t => t.WalletNumber).HasMaxLength(10);
        builder.Property(t => t.Currency).HasMaxLength(50);
        builder.Property(b => b.OrderId).HasMaxLength(50);
        builder.Property(b => b.CommissionRate).HasPrecision(4, 2);
        builder.Property(b => b.ErrorCode).HasMaxLength(50);
        builder.Property(b => b.ErrorMessage).HasMaxLength(256);
        builder.Property(b => b.ThreedSessionId).HasMaxLength(200);
        builder.Property(u => u.Name).HasMaxLength(150);
        builder.Property(b => b.ProvisionNumber).HasMaxLength(50);
        builder.Property(b => b.CardNumber).HasMaxLength(50);
        builder.Property(b => b.CancelDescription).HasMaxLength(300);
        builder.Property(b => b.Fee).HasPrecision(18, 2);
        builder.Property(b => b.CardBrand).HasMaxLength(50);
        builder.Property(b => b.CardType).HasMaxLength(50);
        builder.Property(b => b.AccountKey).HasMaxLength(50);
        builder.Property(b => b.AuthenticationMethod).HasMaxLength(50);
        builder.Property(b => b.Secure3dType).HasMaxLength(50);
        builder.Property(b => b.Description).HasMaxLength(300);
        builder.Property(b => b.BankName).HasMaxLength(300);
        builder.Property(b => b.BankCode).HasDefaultValue(0);
    }
}