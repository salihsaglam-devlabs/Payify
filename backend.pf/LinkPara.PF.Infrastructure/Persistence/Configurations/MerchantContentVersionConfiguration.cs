using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class MerchantContentVersionConfiguration : IEntityTypeConfiguration<MerchantContentVersion>
{
    public void Configure(EntityTypeBuilder<MerchantContentVersion> builder)
    {
        builder.Property(t => t.Title).HasMaxLength(150).IsRequired();
        builder.Property(t => t.LanguageCode).HasMaxLength(10).IsRequired();
        builder.HasIndex(t => t.MerchantContentId);
    }
}