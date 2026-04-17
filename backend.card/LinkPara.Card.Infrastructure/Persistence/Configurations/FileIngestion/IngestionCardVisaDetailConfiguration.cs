using LinkPara.Card.Domain.Entities.FileIngestion.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.FileIngestion;

public class IngestionCardVisaDetailConfiguration : IEntityTypeConfiguration<IngestionCardVisaDetail>
{
    public void Configure(EntityTypeBuilder<IngestionCardVisaDetail> builder)
    {
        ConfigureColumns(builder);

        builder.HasIndex(x => x.FileLineId).IsUnique();
        builder.HasIndex(x => x.Rrn);
        builder.HasIndex(x => x.Arn);
        builder.HasIndex(x => x.CardNo);
        builder.HasIndex(x => x.OceanTxnGuid);

        builder.HasOne(x => x.FileLine)
            .WithOne(x => x.CardVisaDetail)
            .HasForeignKey<IngestionCardVisaDetail>(x => x.FileLineId);
    }

    internal static void ConfigureColumns<T>(EntityTypeBuilder<T> builder) where T : IngestionCardVisaDetail
    {
        builder.Property(x => x.FileLineId);
        builder.Property(x => x.TransactionDate);
        builder.Property(x => x.TransactionTime);
        builder.Property(x => x.ValueDate);
        builder.Property(x => x.EndOfDayDate);
        builder.Property(x => x.CardNo).HasMaxLength(64);
        builder.Property(x => x.OceanTxnGuid);
        builder.Property(x => x.OceanMainTxnGuid);
        builder.Property(x => x.BranchId).HasMaxLength(32);
        builder.Property(x => x.Rrn).HasMaxLength(64);
        builder.Property(x => x.Arn).HasMaxLength(64);
        builder.Property(x => x.ProvisionCode).HasMaxLength(32);
        builder.Property(x => x.Stan);
        builder.Property(x => x.MemberRefNo).HasMaxLength(64);
        builder.Property(x => x.TraceId);
        builder.Property(x => x.Otc);
        builder.Property(x => x.Ots);
        builder.Property(x => x.TxnInstallType).HasConversion<string>();
        builder.Property(x => x.BankingTxnCode).HasMaxLength(32);
        builder.Property(x => x.TxnDescription).HasMaxLength(512);
        builder.Property(x => x.MerchantName).HasMaxLength(256);
        builder.Property(x => x.MerchantCity).HasMaxLength(128);
        builder.Property(x => x.MerchantState).HasMaxLength(64);
        builder.Property(x => x.MerchantCountry).HasMaxLength(64);
        builder.Property(x => x.FinancialType).HasConversion<string>();
        builder.Property(x => x.TxnEffect).HasConversion<string>();
        builder.Property(x => x.TxnSource).HasConversion<string>();
        builder.Property(x => x.TxnRegion).HasConversion<string>();
        builder.Property(x => x.TerminalType).HasConversion<string>();
        builder.Property(x => x.ChannelCode).HasConversion<string>();
        builder.Property(x => x.TerminalId).HasMaxLength(64);
        builder.Property(x => x.MerchantId).HasMaxLength(64);
        builder.Property(x => x.Mcc);
        builder.Property(x => x.AcquirerId);
        builder.Property(x => x.SecurityLevelIndicator);
        builder.Property(x => x.IsTxnSettle).HasConversion<string>();
        builder.Property(x => x.TxnStat).HasConversion<string>();
        builder.Property(x => x.ResponseCode).HasMaxLength(16);
        builder.Property(x => x.IsSuccessfulTxn).HasConversion<string>();
        builder.Property(x => x.TxnOrigin).HasConversion<string>();
        builder.Property(x => x.InstallCount);
        builder.Property(x => x.InstallOrder);
        builder.Property(x => x.OperatorCode).HasMaxLength(32);
        builder.Property(x => x.OriginalAmount).HasPrecision(18, 4);
        builder.Property(x => x.OriginalCurrency);
        builder.Property(x => x.SettlementAmount).HasPrecision(18, 4);
        builder.Property(x => x.SettlementCurrency);
        builder.Property(x => x.CardHolderBillingAmount).HasPrecision(18, 4);
        builder.Property(x => x.CardHolderBillingCurrency);
        builder.Property(x => x.BillingAmount).HasPrecision(18, 4);
        builder.Property(x => x.BillingCurrency);
        builder.Property(x => x.Tax1).HasPrecision(18, 4);
        builder.Property(x => x.Tax2).HasPrecision(18, 4);
        builder.Property(x => x.CashbackAmount).HasPrecision(18, 4);
        builder.Property(x => x.SurchargeAmount).HasPrecision(18, 4);
        builder.Property(x => x.PointType).HasMaxLength(16);
        builder.Property(x => x.BcPoint).HasPrecision(18, 4);
        builder.Property(x => x.McPoint).HasPrecision(18, 4);
        builder.Property(x => x.CcPoint).HasPrecision(18, 4);
        builder.Property(x => x.BcPointAmount).HasPrecision(18, 4);
        builder.Property(x => x.McPointAmount).HasPrecision(18, 4);
        builder.Property(x => x.CcPointAmount).HasPrecision(18, 4);
    }
}
