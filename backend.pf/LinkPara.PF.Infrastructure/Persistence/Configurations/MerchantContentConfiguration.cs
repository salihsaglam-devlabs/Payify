using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class MerchantContentConfiguration : IEntityTypeConfiguration<MerchantContent>
{
    public void Configure(EntityTypeBuilder<MerchantContent> builder)
    {
        builder.Property(t => t.MerchantId).IsRequired();
        builder.Property(t => t.ContentSource).IsRequired();
        builder.Property(t => t.Name).HasMaxLength(50).IsRequired();
        builder.HasIndex(t => t.RecordStatus);
    }
}