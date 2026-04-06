using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class MerchantResponseCodeConfiguration : IEntityTypeConfiguration<MerchantResponseCode>
{
    public void Configure(EntityTypeBuilder<MerchantResponseCode> builder)
    {
        builder.Property(b => b.ResponseCode).IsRequired().HasMaxLength(10);
        builder.Property(b => b.Description).IsRequired().HasMaxLength(100);
        builder.Property(b => b.DisplayMessageTr).IsRequired().HasMaxLength(500);
        builder.Property(b => b.DisplayMessageEn).IsRequired().HasMaxLength(500);
    }
}
