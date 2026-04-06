using LinkPara.PF.Domain.Entities.PhysicalPos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations.PhysicalPos;

public class MerchantDeviceApiKeyConfiguration : IEntityTypeConfiguration<MerchantDeviceApiKey>
{
    public void Configure(EntityTypeBuilder<MerchantDeviceApiKey> builder)
    {
        builder.Property(b => b.PrivateKeyEncrypted).IsRequired().HasMaxLength(100);
        builder.Property(b => b.PublicKey).IsRequired().HasMaxLength(100);
    }
}
