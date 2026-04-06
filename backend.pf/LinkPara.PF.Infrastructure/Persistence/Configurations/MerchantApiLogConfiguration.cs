using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class MerchantApiLogConfiguration : IEntityTypeConfiguration<MerchantApiLog>
{
    public void Configure(EntityTypeBuilder<MerchantApiLog> builder)
    {
        builder.Property(b => b.MerchantId).IsRequired();
        builder.Property(b => b.PaymentType).IsRequired();
        builder.Property(b => b.ErrorCode).IsRequired().HasMaxLength(10);
        builder.Property(b => b.ErrorMessage).IsRequired().HasMaxLength(256);
    }
}
