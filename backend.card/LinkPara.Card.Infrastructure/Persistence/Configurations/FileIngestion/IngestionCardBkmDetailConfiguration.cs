using LinkPara.Card.Domain.Entities.FileIngestion.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.FileIngestion;

public class IngestionCardBkmDetailConfiguration : IEntityTypeConfiguration<IngestionCardBkmDetail>
{
    public void Configure(EntityTypeBuilder<IngestionCardBkmDetail> builder)
    {
        ConfigureColumns(builder);

        builder.HasIndex(x => x.IngestionFileLineId).IsUnique();
        builder.HasIndex(x => x.Rrn);
        builder.HasIndex(x => x.Arn);
        builder.HasIndex(x => x.CardNo);
        builder.HasIndex(x => x.OceanTxnGuid);

        builder.HasOne(x => x.IngestionFileLine)
            .WithOne(x => x.CardBkmDetail)
            .HasForeignKey<IngestionCardBkmDetail>(x => x.IngestionFileLineId);
    }

    internal static void ConfigureColumns<T>(EntityTypeBuilder<T> builder) where T : IngestionCardBkmDetail
    {
        builder.Property(x => x.IngestionFileLineId).HasColumnName("file_line_id");
        builder.Property(x => x.TransactionDate).HasColumnName("transaction_date");
        builder.Property(x => x.TransactionTime).HasColumnName("transaction_time");
        builder.Property(x => x.ValueDate).HasColumnName("value_date");
        builder.Property(x => x.EndOfDayDate).HasColumnName("end_of_day_date");
        builder.Property(x => x.CardNo).HasColumnName("card_no").HasMaxLength(64);
        builder.Property(x => x.OceanTxnGuid).HasColumnName("ocean_txn_guid");
        builder.Property(x => x.OceanMainTxnGuid).HasColumnName("ocean_main_txn_guid");
        builder.Property(x => x.BranchId).HasColumnName("branch_id").HasMaxLength(32);
        builder.Property(x => x.Rrn).HasColumnName("rrn").HasMaxLength(64);
        builder.Property(x => x.Arn).HasColumnName("arn").HasMaxLength(64);
        builder.Property(x => x.ProvisionCode).HasColumnName("provision_code").HasMaxLength(32);
        builder.Property(x => x.Stan).HasColumnName("stan");
        builder.Property(x => x.MemberRefNo).HasColumnName("member_ref_no").HasMaxLength(64);
        builder.Property(x => x.TraceId).HasColumnName("trace_id");
        builder.Property(x => x.Otc).HasColumnName("otc");
        builder.Property(x => x.Ots).HasColumnName("ots");
        builder.Property(x => x.TxnInstallType).HasColumnName("txn_install_type").HasConversion<string>().HasMaxLength(8);
        builder.Property(x => x.BankingTxnCode).HasColumnName("banking_txn_code").HasMaxLength(32);
        builder.Property(x => x.TxnDescription).HasColumnName("txn_description").HasMaxLength(512);
        builder.Property(x => x.MerchantName).HasColumnName("merchant_name").HasMaxLength(256);
        builder.Property(x => x.MerchantCity).HasColumnName("merchant_city").HasMaxLength(128);
        builder.Property(x => x.MerchantState).HasColumnName("merchant_state").HasMaxLength(64);
        builder.Property(x => x.MerchantCountry).HasColumnName("merchant_country").HasMaxLength(64);
        builder.Property(x => x.FinancialType).HasColumnName("financial_type").HasConversion<string>().HasMaxLength(8);
        builder.Property(x => x.TxnEffect).HasColumnName("txn_effect").HasConversion<string>().HasMaxLength(8);
        builder.Property(x => x.TxnSource).HasColumnName("txn_source").HasConversion<string>().HasMaxLength(8);
        builder.Property(x => x.TxnRegion).HasColumnName("txn_region").HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.TerminalType).HasColumnName("terminal_type").HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.ChannelCode).HasColumnName("channel_code").HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.TerminalId).HasColumnName("terminal_id").HasMaxLength(64);
        builder.Property(x => x.MerchantId).HasColumnName("merchant_id").HasMaxLength(64);
        builder.Property(x => x.Mcc).HasColumnName("mcc");
        builder.Property(x => x.AcquirerId).HasColumnName("acquirer_id");
        builder.Property(x => x.SecurityLevelIndicator).HasColumnName("security_level_indicator");
        builder.Property(x => x.IsTxnSettle).HasColumnName("is_txn_settle").HasConversion<string>().HasMaxLength(8);
        builder.Property(x => x.TxnStat).HasColumnName("txn_stat").HasConversion<string>().HasMaxLength(8);
        builder.Property(x => x.ResponseCode).HasColumnName("response_code").HasMaxLength(16);
        builder.Property(x => x.IsSuccessfulTxn).HasColumnName("is_successful_txn").HasConversion<string>().HasMaxLength(8);
        builder.Property(x => x.TxnOrigin).HasColumnName("txn_origin").HasConversion<string>().HasMaxLength(8);
        builder.Property(x => x.InstallCount).HasColumnName("install_count");
        builder.Property(x => x.InstallOrder).HasColumnName("install_order");
        builder.Property(x => x.OperatorCode).HasColumnName("operator_code").HasMaxLength(32);
        builder.Property(x => x.OriginalAmount).HasColumnName("original_amount").HasPrecision(18, 4);
        builder.Property(x => x.OriginalCurrency).HasColumnName("original_currency");
        builder.Property(x => x.SettlementAmount).HasColumnName("settlement_amount").HasPrecision(18, 4);
        builder.Property(x => x.SettlementCurrency).HasColumnName("settlement_currency");
        builder.Property(x => x.CardHolderBillingAmount).HasColumnName("cardholder_billing_amount").HasPrecision(18, 4);
        builder.Property(x => x.CardHolderBillingCurrency).HasColumnName("cardholder_billing_currency");
        builder.Property(x => x.BillingAmount).HasColumnName("billing_amount").HasPrecision(18, 4);
        builder.Property(x => x.BillingCurrency).HasColumnName("billing_currency");
        builder.Property(x => x.Tax1).HasColumnName("tax1").HasPrecision(18, 4);
        builder.Property(x => x.Tax2).HasColumnName("tax2").HasPrecision(18, 4);
        builder.Property(x => x.CashbackAmount).HasColumnName("cashback_amount").HasPrecision(18, 4);
        builder.Property(x => x.SurchargeAmount).HasColumnName("surcharge_amount").HasPrecision(18, 4);
        builder.Property(x => x.PointType).HasColumnName("point_type").HasMaxLength(16);
        builder.Property(x => x.BcPoint).HasColumnName("bc_point").HasPrecision(18, 4);
        builder.Property(x => x.McPoint).HasColumnName("mc_point").HasPrecision(18, 4);
        builder.Property(x => x.CcPoint).HasColumnName("cc_point").HasPrecision(18, 4);
        builder.Property(x => x.BcPointAmount).HasColumnName("bc_point_amount").HasPrecision(18, 4);
        builder.Property(x => x.McPointAmount).HasColumnName("mc_point_amount").HasPrecision(18, 4);
        builder.Property(x => x.CcPointAmount).HasColumnName("cc_point_amount").HasPrecision(18, 4);
    }
}
