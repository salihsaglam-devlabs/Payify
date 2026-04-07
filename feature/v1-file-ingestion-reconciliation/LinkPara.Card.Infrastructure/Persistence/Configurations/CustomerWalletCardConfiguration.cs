using LinkPara.Card.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations;

public class CustomerWalletCardConfiguration : IEntityTypeConfiguration<CustomerWalletCard>
{
    public void Configure(EntityTypeBuilder<CustomerWalletCard> builder)
    {
        builder.Property(x => x.BankingCustomerNo).HasMaxLength(16);
        builder.Property(x => x.WalletNumber).HasMaxLength(50);
        builder.Property(x => x.CardNumber).HasMaxLength(50);
        builder.Property(x => x.ProductCode).HasMaxLength(50);
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
    }
}
