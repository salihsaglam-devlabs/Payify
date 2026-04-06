using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class MerchantBlockageDetailConfiguration : IEntityTypeConfiguration<MerchantBlockageDetail>
{
    public void Configure(EntityTypeBuilder<MerchantBlockageDetail> builder)
    {
        builder.Property(b => b.PostingDate).IsRequired().HasColumnType("date");
        builder.Property(b => b.MerchantId).IsRequired();
        builder.Property(b => b.TotalAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.BlockageAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.RemainingAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.BlockageStatus).IsRequired();
        
        builder
            .HasOne(s => s.MerchantBlockage)
            .WithMany(s=> s.MerchantBlockageDetails)
            .HasForeignKey(s => s.MerchantBlockageId)
            .OnDelete(DeleteBehavior.Restrict);

    }
}