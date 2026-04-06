using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class MerchantWalletConfiguration : IEntityTypeConfiguration<MerchantWallet>
{
    public void Configure(EntityTypeBuilder<MerchantWallet> builder)
    {
        builder.Property(b => b.WalletNumber).IsRequired().HasMaxLength(26);
        builder.Property(b => b.MerchantId).IsRequired();
    }
}
