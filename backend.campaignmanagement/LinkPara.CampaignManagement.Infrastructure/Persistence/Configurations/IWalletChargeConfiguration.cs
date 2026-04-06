using LinkPara.CampaignManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.CampaignManagement.Infrastructure.Persistence.Configurations;

public class IWalletChargeConfiguration : IEntityTypeConfiguration<IWalletCharge>
{
    public void Configure(EntityTypeBuilder<IWalletCharge> builder)
    {
        builder.Property(s => s.WalletId).HasMaxLength(100).IsRequired();
        builder.Property(s => s.TerminalId).IsRequired();
        builder.Property(s => s.TerminalName).HasMaxLength(500).IsRequired();
        builder.Property(s => s.Amount).HasPrecision(18, 4).IsRequired();
        builder.Property(s => s.CurrencyCode).IsRequired();
        builder.Property(s => s.QrCode).IsRequired();
        builder.Property(s => s.WalletNumber).HasMaxLength(50).IsRequired();
        builder.Property(s => s.MerchantName).HasMaxLength(500);
        builder.Property(s => s.MerchantBranchName).HasMaxLength(500);

        builder
        .HasOne(s => s.IWalletQrCode)
        .WithMany()
        .HasForeignKey(s => s.IWalletQrCodeId)
        .HasPrincipalKey(s => s.Id)
        .OnDelete(DeleteBehavior.Restrict);
    }
}