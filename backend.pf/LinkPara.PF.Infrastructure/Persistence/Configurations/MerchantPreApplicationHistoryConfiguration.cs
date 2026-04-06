using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class MerchantPreApplicationHistoryConfiguration : IEntityTypeConfiguration<MerchantPreApplicationHistory>
{
    public void Configure(EntityTypeBuilder<MerchantPreApplicationHistory> builder)
    {
        builder.Property(b => b.UserId).IsRequired();
        builder.Property(b => b.UserName).IsRequired().HasMaxLength(50);
        builder.Property(b => b.MerchantPreApplicationId).IsRequired();
        builder.Property(b => b.OperationDate).IsRequired();
        builder.Property(b => b.OperationType).IsRequired();
    }
}