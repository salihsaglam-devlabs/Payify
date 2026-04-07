using LinkPara.Card.Domain.Entities.FileIngestion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations;

public class ClearingRecordConfiguration : IEntityTypeConfiguration<ClearingRecord>
{
    public void Configure(EntityTypeBuilder<ClearingRecord> builder)
    {
        builder.Property(x => x.Provider)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(x => x.OceanTxnGuid).HasMaxLength(32);
        builder.Property(x => x.ClrNo).HasMaxLength(32);
        builder.Property(x => x.Rrn).HasMaxLength(24);
        builder.Property(x => x.Arn).HasMaxLength(32);
        builder.Property(x => x.ProvisionCode).HasMaxLength(16);
        builder.Property(x => x.CardNo).HasMaxLength(32);
        builder.Property(x => x.MccCode).HasMaxLength(8);
        builder.Property(x => x.ControlStat).HasMaxLength(2);
        builder.Property(x => x.CorrelationKey).HasMaxLength(256);

        builder.HasIndex(x => new { x.ClrNo, x.ControlStat, x.Provider });
        builder.HasIndex(x => new { x.Rrn, x.Arn, x.ProvisionCode, x.MccCode, x.SourceAmount, x.SourceCurrency });
        builder.HasIndex(x => x.CorrelationKey);
        builder.HasIndex(x => new { x.ControlStat, x.CardNo, x.CreateDate });
        builder.HasIndex(x => new { x.CorrelationKey, x.CreateDate });

        builder
            .HasOne(x => x.ImportedFileRow)
            .WithOne(x => x.ClearingRecord)
            .HasForeignKey<ClearingRecord>(x => x.ImportedFileRowId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
