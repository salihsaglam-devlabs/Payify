using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class PostingItemConfiguration : IEntityTypeConfiguration<PostingItem>
{
    public void Configure(EntityTypeBuilder<PostingItem> builder)
    {
        builder.Property(b => b.MerchantId).IsRequired();
        builder.Property(b => b.PostingDate).IsRequired().HasColumnType("date");
        builder.Property(b => b.BatchStatus).IsRequired();

        builder.HasIndex(b => b.MerchantId);
        builder.HasIndex(b => new { b.MerchantId, b.PostingDate });
    }
}