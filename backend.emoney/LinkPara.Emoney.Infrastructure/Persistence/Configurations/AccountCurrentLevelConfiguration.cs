using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class AccountCurrentLevelConfigurationConfiguration : IEntityTypeConfiguration<AccountCurrentLevel>
{
    public void Configure(EntityTypeBuilder<AccountCurrentLevel> builder)
    {
        builder.Property(s => s.AccountId).IsRequired();
        builder.Property(s => s.LevelDate).IsRequired();
        builder.Property(s => s.DailyInternalTransferAmount).IsRequired().HasPrecision(18, 2);
        builder.Property(s => s.MonthlyInternalTransferAmount).IsRequired().HasPrecision(18, 2);
        builder.Property(s => s.DailyDepositAmount).IsRequired().HasPrecision(18, 2);
        builder.Property(s => s.MonthlyDepositAmount).IsRequired().HasPrecision(18, 2);
        builder.Property(s => s.DailyWithdrawalAmount).IsRequired().HasPrecision(18, 2);
        builder.Property(s => s.MonthlyWithdrawalAmount).IsRequired().HasPrecision(18, 2);
        builder.Property(s => s.DailyInternationalTransferAmount).IsRequired().HasPrecision(18, 2);
        builder.Property(s => s.MonthlyInternationalTransferAmount).IsRequired().HasPrecision(18, 2);
        builder.Property(s => s.DailyOnUsPaymentAmount).IsRequired().HasPrecision(18, 2);
        builder.Property(s => s.MonthlyOnUsPaymentAmount).IsRequired().HasPrecision(18, 2);
        builder.Property(s => s.DailyInternalTransferCount).IsRequired();
        builder.Property(s => s.MonthlyInternalTransferCount).IsRequired();
        builder.Property(s => s.DailyDepositCount).IsRequired();
        builder.Property(s => s.MonthlyDepositCount).IsRequired();
        builder.Property(s => s.DailyWithdrawalCount).IsRequired();
        builder.Property(s => s.MonthlyWithdrawalCount).IsRequired();
        builder.Property(s => s.DailyOwnIbanWithdrawalCount).IsRequired();
        builder.Property(s => s.MonthlyOwnIbanWithdrawalCount).IsRequired();
        builder.Property(s => s.DailyOtherIbanWithdrawalCount).IsRequired();
        builder.Property(s => s.MonthlyOtherIbanWithdrawalCount).IsRequired();
        builder.Property(s => s.DailyInternationalTransferCount).IsRequired();
        builder.Property(s => s.MonthlyInternationalTransferCount).IsRequired();
        builder.Property(s => s.DailyDistinctOtherIbanWithdrawalCount).IsRequired();
        builder.Property(s => s.DailyOnUsPaymentCount).IsRequired();
        builder.Property(s => s.MonthlyOnUsPaymentCount).IsRequired();
        builder.Property(s => s.DailyOtherIbanWithdrawalAmount).IsRequired().HasPrecision(18, 2);
        builder.Property(s => s.MonthlyDistinctOtherIbanWithdrawalCount).IsRequired();
        builder.Property(s => s.MonthlyOtherIbanWithdrawalAmount).IsRequired().HasPrecision(18, 2);
        builder.Property(s => s.CurrencyCode).IsRequired();
        builder.HasIndex(s => s.CurrencyCode);

        builder
            .HasOne(s => s.Currency)
            .WithMany()
            .HasForeignKey(s => s.CurrencyCode)
            .HasPrincipalKey(s => s.Code)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
