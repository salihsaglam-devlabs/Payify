using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class MerchantDueConfiguration : IEntityTypeConfiguration<MerchantDue>
{
    public void Configure(EntityTypeBuilder<MerchantDue> builder)
    {
        builder.Property(b => b.MerchantId).IsRequired();
        builder.Property(b => b.DueProfileId).IsRequired();
        builder.Property(b => b.TotalExecutionCount).IsRequired();
        builder.Property(b => b.LastExecutionDate).IsRequired();
    }
}