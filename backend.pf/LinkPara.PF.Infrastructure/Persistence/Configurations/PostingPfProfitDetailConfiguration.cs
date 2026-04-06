using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class PostingPfProfitDetailConfiguration : IEntityTypeConfiguration<PostingPfProfitDetail>
{
    public void Configure(EntityTypeBuilder<PostingPfProfitDetail> builder)
    {
        builder.Property(b => b.AcquireBankCode).IsRequired();
        builder.Property(b => b.BankName).IsRequired().HasMaxLength(20);
        builder.Property(b => b.BankPayingAmount).IsRequired().HasPrecision(18, 4);

        builder
            .HasOne(b => b.PostingPfProfit)
            .WithMany(b => b.PostingPfProfitDetails)
            .HasForeignKey(b => b.PostingPfProfitId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}