using LinkPara.Card.Domain.Entities.FileIngestion.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.FileIngestion;

public class IngestionClearingVisaDetailConfiguration : IEntityTypeConfiguration<IngestionClearingVisaDetail>
{
    public void Configure(EntityTypeBuilder<IngestionClearingVisaDetail> builder)
    {
        ConfigureColumns(builder);

        builder.HasIndex(x => x.FileLineId).IsUnique();
        builder.HasIndex(x => x.Rrn);
        builder.HasIndex(x => x.Arn);
        builder.HasIndex(x => x.CardNo);
        builder.HasIndex(x => x.OceanTxnGuid);

        builder.HasOne(x => x.FileLine)
            .WithOne(x => x.ClearingVisaDetail)
            .HasForeignKey<IngestionClearingVisaDetail>(x => x.FileLineId);
    }

    internal static void ConfigureColumns<T>(EntityTypeBuilder<T> builder) where T : IngestionClearingVisaDetail
    {
        builder.Property(x => x.FileLineId);
        builder.Property(x => x.TxnType).HasConversion<string>();
        builder.Property(x => x.IoDate).HasMaxLength(32);
        builder.Property(x => x.IoFlag).HasConversion<string>();
        builder.Property(x => x.OceanTxnGuid);
        builder.Property(x => x.ClrNo);
        builder.Property(x => x.Rrn).HasMaxLength(64);
        builder.Property(x => x.Arn).HasMaxLength(64);
        builder.Property(x => x.ReasonCode).HasMaxLength(32);
        builder.Property(x => x.Reserved).HasMaxLength(256);
        builder.Property(x => x.ProvisionCode).HasMaxLength(32);
        builder.Property(x => x.CardNo).HasMaxLength(64);
        builder.Property(x => x.CardDci).HasConversion<string>();
        builder.Property(x => x.MccCode).HasMaxLength(16);
        builder.Property(x => x.Tc).HasMaxLength(16);
        builder.Property(x => x.UsageCode).HasMaxLength(16);
        builder.Property(x => x.DisputeCode).HasConversion<string>().HasMaxLength(100);
        builder.Property(x => x.ControlStat).HasConversion<string>();
        builder.Property(x => x.SourceAmount).HasPrecision(18, 4);
        builder.Property(x => x.SourceCurrency);
        builder.Property(x => x.DestinationAmount).HasPrecision(18, 4);
        builder.Property(x => x.DestinationCurrency);
        builder.Property(x => x.CashbackAmount).HasPrecision(18, 4);
        builder.Property(x => x.ReimbursementAmount).HasPrecision(18, 4);
        builder.Property(x => x.ReimbursementAttribute).HasMaxLength(64);
        builder.Property(x => x.MicrofilmNumber);
        builder.Property(x => x.MerchantCity).HasMaxLength(128);
        builder.Property(x => x.MerchantName).HasMaxLength(256);
        builder.Property(x => x.CardAcceptorId).HasMaxLength(64);
        builder.Property(x => x.TxnDate);
        builder.Property(x => x.TxnTime);
        builder.Property(x => x.FileId).HasMaxLength(64);
    }
}
