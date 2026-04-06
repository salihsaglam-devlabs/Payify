using LinkPara.CampaignManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.CampaignManagement.Infrastructure.Persistence.Configurations;

public class IWalletQrCodeConfiguration : IEntityTypeConfiguration<IWalletQrCode>
{
    public void Configure(EntityTypeBuilder<IWalletQrCode> builder)
    {
        builder.Property(s => s.WalletNumber).HasMaxLength(100).IsRequired();
        builder.Property(s => s.Message).HasMaxLength(800);
        builder.Property(s => s.CardNumber).HasMaxLength(100).IsRequired();

        builder
            .HasOne(s => s.IWalletCard)
            .WithMany()
            .HasForeignKey(s => s.IWalletCardId)
            .HasPrincipalKey(s => s.Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.QrCode);
    }
}
