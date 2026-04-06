using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class PostingTransferErrorConfiguration : IEntityTypeConfiguration<PostingTransferError>
{
    public void Configure(EntityTypeBuilder<PostingTransferError> builder)
    {
        builder.Property(b => b.MerchantId);
        builder.Property(b => b.MerchantTransactionId).IsRequired();
        builder.Property(b => b.ErrorMessage).IsRequired().HasMaxLength(500);
        builder.Property(b => b.PostingDate).IsRequired();
        builder.Property(b => b.StackTrace).HasMaxLength(2000);
    }
}