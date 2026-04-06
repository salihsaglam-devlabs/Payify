using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class PostingPfProfitConfiguration : IEntityTypeConfiguration<PostingPfProfit>
{
    public void Configure(EntityTypeBuilder<PostingPfProfit> builder)
    {
        builder.Property(b => b.PaymentDate).IsRequired().HasColumnType("date");
        builder.Property(b => b.TotalPayingAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.ProtectionTransferAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.AmountWithoutBankCommission).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.TotalPfNetCommissionAmount).IsRequired().HasPrecision(18, 4);
        
        builder
            .HasMany(b => b.PostingPfProfitDetails)
            .WithOne(b => b.PostingPfProfit)
            .IsRequired();
    }
}