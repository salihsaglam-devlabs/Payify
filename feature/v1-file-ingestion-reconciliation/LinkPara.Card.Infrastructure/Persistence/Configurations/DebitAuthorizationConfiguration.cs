using LinkPara.Card.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations;

public class DebitAuthorizationConfiguration : IEntityTypeConfiguration<DebitAuthorization>
{
    public void Configure(EntityTypeBuilder<DebitAuthorization> builder)
    {
        builder.Property(x => x.TransactionAmount).HasColumnType("numeric");
        builder.Property(x => x.BillingAmount).HasColumnType("numeric");
        builder.Property(x => x.ReplacementTransactionAmount).HasColumnType("numeric");
        builder.Property(x => x.ReplacementBillingAmount).HasColumnType("numeric");
        builder.Property(x => x.TransactionSource).HasColumnType("character(1)").IsRequired();
        builder.Property(x => x.CardDci).HasColumnType("character(1)").IsRequired();
        builder.Property(x => x.CardBrand).HasColumnType("character(1)").IsRequired();
        builder.Property(x => x.EntryType).HasColumnType("character(1)").IsRequired();
        builder.Property(x => x.TransferInformationType).HasColumnType("character(1)");

        builder.HasIndex(x => x.OceanTxnGuid);
    }
}
