using LinkPara.Card.Domain.Entities.FileIngestion.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.FileIngestion;

public class IngestionClearingMscDetailConfiguration : IEntityTypeConfiguration<IngestionClearingMscDetail>
{
    public void Configure(EntityTypeBuilder<IngestionClearingMscDetail> builder)
    {
        ConfigureColumns(builder);

        builder.HasIndex(x => x.IngestionFileLineId).IsUnique();
        builder.HasIndex(x => x.Rrn);
        builder.HasIndex(x => x.Arn);
        builder.HasIndex(x => x.CardNo);
        builder.HasIndex(x => x.OceanTxnGuid);

        builder.HasOne(x => x.IngestionFileLine)
            .WithOne(x => x.ClearingMscDetail)
            .HasForeignKey<IngestionClearingMscDetail>(x => x.IngestionFileLineId);
    }

    internal static void ConfigureColumns<T>(EntityTypeBuilder<T> builder) where T : IngestionClearingMscDetail
    {
        builder.Property(x => x.IngestionFileLineId).HasColumnName("file_line_id");
        builder.Property(x => x.TxnType).HasColumnName("txn_type").HasConversion<string>().HasMaxLength(8);
        builder.Property(x => x.IoDate).HasColumnName("io_date").HasMaxLength(32);
        builder.Property(x => x.IoFlag).HasColumnName("io_flag").HasConversion<string>().HasMaxLength(8);
        builder.Property(x => x.OceanTxnGuid).HasColumnName("ocean_txn_guid");
        builder.Property(x => x.ClrNo).HasColumnName("clr_no");
        builder.Property(x => x.Rrn).HasColumnName("rrn").HasMaxLength(64);
        builder.Property(x => x.Arn).HasColumnName("arn").HasMaxLength(64);
        builder.Property(x => x.ReasonCode).HasColumnName("reason_code").HasMaxLength(32);
        builder.Property(x => x.Reserved).HasColumnName("reserved").HasMaxLength(256);
        builder.Property(x => x.ProvisionCode).HasColumnName("provision_code").HasMaxLength(32);
        builder.Property(x => x.CardNo).HasColumnName("card_no").HasMaxLength(64);
        builder.Property(x => x.CardDci).HasColumnName("card_dci").HasConversion<string>().HasMaxLength(8);
        builder.Property(x => x.MccCode).HasColumnName("mcc_code").HasMaxLength(16);
        builder.Property(x => x.Mtid).HasColumnName("mtid").HasMaxLength(16);
        builder.Property(x => x.FunctionCode).HasColumnName("function_code").HasMaxLength(16);
        builder.Property(x => x.ProcessCode).HasColumnName("process_code").HasMaxLength(16);
        builder.Property(x => x.ReversalIndicator).HasColumnName("reversal_indicator").HasMaxLength(16);
        builder.Property(x => x.DisputeCode).HasColumnName("dispute_code").HasConversion<string>().HasMaxLength(64);
        builder.Property(x => x.ControlStat).HasColumnName("control_stat").HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.SourceAmount).HasColumnName("source_amount").HasPrecision(18, 4);
        builder.Property(x => x.SourceCurrency).HasColumnName("source_currency");
        builder.Property(x => x.DestinationAmount).HasColumnName("destination_amount").HasPrecision(18, 4);
        builder.Property(x => x.DestinationCurrency).HasColumnName("destination_currency");
        builder.Property(x => x.CashbackAmount).HasColumnName("cashback_amount").HasPrecision(18, 4);
        builder.Property(x => x.ReimbursementAmount).HasColumnName("reimbursement_amount").HasPrecision(18, 4);
        builder.Property(x => x.ReimbursementAttribute).HasColumnName("reimbursement_attribute").HasMaxLength(64);
        builder.Property(x => x.AncillaryTransactionCode).HasColumnName("ancillary_transaction_code").HasMaxLength(32);
        builder.Property(x => x.AncillaryTransactionAmount).HasColumnName("ancillary_transaction_amount").HasMaxLength(64);
        builder.Property(x => x.MicrofilmNumber).HasColumnName("microfilm_number");
        builder.Property(x => x.MerchantCity).HasColumnName("merchant_city").HasMaxLength(128);
        builder.Property(x => x.MerchantName).HasColumnName("merchant_name").HasMaxLength(256);
        builder.Property(x => x.CardAcceptorId).HasColumnName("card_acceptor_id").HasMaxLength(64);
        builder.Property(x => x.TxnDate).HasColumnName("txn_date");
        builder.Property(x => x.TxnTime).HasColumnName("txn_time");
        builder.Property(x => x.FileId).HasColumnName("file_id").HasMaxLength(64);
    }
}
