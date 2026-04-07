using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using LinkPara.Card.Domain.Entities;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations
{
    public class CustomerWalletCardConfiguration : IEntityTypeConfiguration<CustomerWalletCard>
    {
        public void Configure(EntityTypeBuilder<CustomerWalletCard> builder)
        {
            builder.Property(s => s.BankingCustomerNo).HasMaxLength(16);
            builder.Property(s => s.WalletNumber).HasMaxLength(50);
            builder.Property(s => s.CardNumber).HasMaxLength(50);
            builder.Property(s => s.CardName).HasMaxLength(30);
            builder.Property(s => s.ProductCode).HasMaxLength(50);
        }
    }

}
