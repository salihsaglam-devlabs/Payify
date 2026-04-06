using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class SavedAccountConfiguration : IEntityTypeConfiguration<SavedAccount>
{
    public void Configure(EntityTypeBuilder<SavedAccount> builder)
    {
        builder.Property(t => t.UserId).IsRequired();
        builder.Property(t => t.Tag).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Type).IsRequired().HasMaxLength(50);
        builder.Property(t => t.ReceiverName).IsRequired().HasMaxLength(200);

        builder.HasDiscriminator(b => b.Type)
            .HasValue<SavedWalletAccount>("Wallet")
            .HasValue<SavedBankAccount>("BankAccount");
    }
}