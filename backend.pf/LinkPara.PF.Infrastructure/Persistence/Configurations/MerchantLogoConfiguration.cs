using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class MerchantLogoConfiguration : IEntityTypeConfiguration<MerchantLogo>
{
    public void Configure(EntityTypeBuilder<MerchantLogo> builder)
    {
        builder.Property(t => t.ContentType).HasMaxLength(100).IsRequired();
        builder.HasIndex(t => t.MerchantId);
    }
}