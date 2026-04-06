using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class TierLevelConfiguration : IEntityTypeConfiguration<TierLevel>
{
    public void Configure(EntityTypeBuilder<TierLevel> builder)
    {
        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
        builder.Property(s => s.TierLevelType).IsRequired().HasMaxLength(50);
        builder.Property(s => s.MaxBalanceLimitEnabled).IsRequired();
        builder.Property(s => s.MaxBalance).IsRequired().HasPrecision(18, 2);
        builder.Property(s => s.MaxInternalTransferLimitEnabled).IsRequired();
        builder.Property(s => s.DailyMaxInternalTransferAmount).IsRequired().HasPrecision(18, 2);
        builder.Property(s => s.MonthlyMaxInternalTransferAmount).IsRequired().HasPrecision(18, 2);
        builder.Property(s => s.MaxDepositLimitEnabled).IsRequired();
        builder.Property(s => s.DailyMaxDepositAmount).IsRequired().HasPrecision(18, 2);
        builder.Property(s => s.MonthlyMaxDepositAmount).IsRequired().HasPrecision(18, 2);
        builder.Property(s => s.MaxWithdrawalLimitEnabled).IsRequired();
        builder.Property(s => s.DailyMaxWithdrawalAmount).IsRequired().HasPrecision(18, 2);
        builder.Property(s => s.MonthlyMaxWithdrawalAmount).IsRequired().HasPrecision(18, 2);
        builder.Property(s => s.MaxInternationalTransferLimitEnabled).IsRequired();
        builder.Property(s => s.DailyMaxInternationalTransferAmount).IsRequired().HasPrecision(18, 2);
        builder.Property(s => s.MonthlyMaxInternationalTransferAmount).IsRequired().HasPrecision(18, 2);
        builder.Property(s => s.MaxOnUsPaymentLimitEnabled).IsRequired();
        builder.Property(s => s.DailyMaxOnUsPaymentAmount).IsRequired().HasPrecision(18, 2);
        builder.Property(s => s.MonthlyMaxOnUsPaymentAmount).IsRequired().HasPrecision(18, 2);
        builder.Property(s => s.DailyMaxDepositCount).IsRequired();
        builder.Property(s => s.MonthlyMaxDepositCount).IsRequired();
        builder.Property(s => s.DailyMaxInternalTransferCount).IsRequired();
        builder.Property(s => s.MonthlyMaxInternalTransferCount).IsRequired();
        builder.Property(s => s.DailyMaxDepositCount).IsRequired();
        builder.Property(s => s.MonthlyMaxDepositCount).IsRequired();
        builder.Property(s => s.DailyMaxWithdrawalCount).IsRequired();
        builder.Property(s => s.MonthlyMaxWithdrawalCount).IsRequired();
        builder.Property(s => s.DailyMaxInternationalTransferCount).IsRequired();
        builder.Property(s => s.MonthlyMaxInternationalTransferCount).IsRequired();
        builder.Property(s => s.MaxOwnIbanWithdrawalLimitEnabled).IsRequired();
        builder.Property(s => s.DailyMaxOwnIbanWithdrawalCount).IsRequired();
        builder.Property(s => s.MonthlyMaxOwnIbanWithdrawalCount).IsRequired();
        builder.Property(s => s.MaxOtherIbanWithdrawalLimitEnabled).IsRequired();
        builder.Property(s => s.DailyMaxOtherIbanWithdrawalCount).IsRequired();
        builder.Property(s => s.MonthlyMaxOtherIbanWithdrawalCount).IsRequired();
        builder.Property(s => s.DailyMaxOtherIbanWithdrawalAmount).IsRequired().HasPrecision(18, 2);
        builder.Property(s => s.MonthlyMaxOtherIbanWithdrawalAmount).IsRequired().HasPrecision(18, 2);
        builder.Property(s => s.DailyMaxDistinctOtherIbanWithdrawalCount).IsRequired();
        builder.Property(s => s.MonthlyMaxDistinctOtherIbanWithdrawalCount).IsRequired();
        
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
