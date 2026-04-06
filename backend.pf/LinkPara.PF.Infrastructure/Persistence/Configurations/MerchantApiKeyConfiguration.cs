using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class MerchantApiKeyConfiguration : IEntityTypeConfiguration<MerchantApiKey>
{
    public void Configure(EntityTypeBuilder<MerchantApiKey> builder)
    {
        builder.Property(b => b.PrivateKeyEncrypted).IsRequired().HasMaxLength(100);
        builder.Property(b => b.PublicKey).IsRequired().HasMaxLength(100);
    }
}
