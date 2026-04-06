using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.HasIndex(s => s.WalletNumber).IsUnique();

        builder.Property(t => t.WalletNumber).HasMaxLength(10).IsRequired();
        builder.Property(t => t.FriendlyName).IsRequired().HasMaxLength(50);
        builder.Property(t => t.CurrentBalanceCredit).HasPrecision(18, 2);
        builder.Property(t => t.CurrentBalanceCash).HasPrecision(18, 2);
        builder.Property(t => t.BlockedBalance).HasPrecision(18, 2);
        builder.Property(t => t.BlockedBalanceCredit).HasPrecision(18, 2);
        builder.Property(t => t.CurrencyCode).IsRequired();
        builder.HasIndex(t => t.CurrencyCode);
        builder.Property(s => s.AccountId).HasDefaultValue(Guid.Empty);

        builder
            .HasOne(s => s.Currency)
            .WithMany()
            .HasForeignKey(s => s.CurrencyCode)
            .HasPrincipalKey(s => s.Code)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(t => t.AvailableBalance);
        builder.Ignore(t => t.DomainEvents);
        builder.Ignore(t => t.AvailableBalanceCredit);
        builder.Ignore(t => t.AvailableBalanceCash);
    }
}