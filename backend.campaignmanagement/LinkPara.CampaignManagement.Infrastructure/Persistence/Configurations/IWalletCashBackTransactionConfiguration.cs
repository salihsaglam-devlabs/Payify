using LinkPara.CampaignManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.CampaignManagement.Infrastructure.Persistence.Configurations;

public class IWalletCashbackTransactionConfiguration : IEntityTypeConfiguration<IWalletCashbackTransaction>
{
    public void Configure(EntityTypeBuilder<IWalletCashbackTransaction> builder)
    {
        builder.Property(s => s.Oid).IsRequired();
        builder.Property(s => s.Amount).HasPrecision(18, 4).IsRequired();
        builder.Property(s => s.Balance).HasPrecision(18, 4).IsRequired();
        builder.Property(s => s.VatRate).HasPrecision(18, 4); 
        builder.Property(s => s.CommissionRate).HasPrecision(18, 4).IsRequired();
        builder.Property(s => s.CommissionAmount).HasPrecision(18, 4).IsRequired();
        builder.Property(s => s.ExternalStatus).IsRequired().HasMaxLength(50);
        builder.Property(s => s.ExternalOrderId).IsRequired();
        builder.Property(s => s.IWalletCardId).IsRequired();
        builder.Property(s => s.WalletId).IsRequired();
        builder.Property(s => s.MerchantName).HasMaxLength(250);
        builder.Property(s => s.MerchantBranchName).HasMaxLength(250);
        builder.Property(s => s.PosId).IsRequired();
        builder.Property(s => s.CustomerId).IsRequired();
        builder.Property(s => s.CustomerBranchId).IsRequired();
        builder.Property(s => s.LoadType).HasMaxLength(50);
        builder.Property(s => s.WalletNumber).HasMaxLength(50);
        builder.Property(s => s.HashData).HasMaxLength(500);

        builder
            .HasOne(s => s.IWalletCharge)
            .WithMany()
            .HasForeignKey(s => s.IWalletChargeId)
            .HasPrincipalKey(s => s.Id)
            .OnDelete(DeleteBehavior.Restrict);
    }
}