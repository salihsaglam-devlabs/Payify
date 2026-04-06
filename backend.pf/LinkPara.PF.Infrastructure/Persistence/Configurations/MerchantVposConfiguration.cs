using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class MerchantVposConfiguration : IEntityTypeConfiguration<MerchantVpos>
{
    public void Configure(EntityTypeBuilder<MerchantVpos> builder)
    {
        builder.Property(b => b.MerchantId).IsRequired();
        builder.Property(b => b.VposId).IsRequired();
        builder.Property(b => b.Priority).IsRequired();
        builder.Property(b => b.SubMerchantCode).HasMaxLength(50);
        builder.Property(b => b.Password).HasMaxLength(50);
        builder.Property(b => b.TerminalNo).HasMaxLength(20);
        builder.Property(b => b.ProviderKey).HasMaxLength(50);
        builder.Property(b => b.ApiKey).HasMaxLength(50);
        builder.Property(b => b.BkmReferenceNumber).HasMaxLength(50);
        builder.Property(b => b.ServiceProviderPspMerchantId).HasMaxLength(100);
    }
}
