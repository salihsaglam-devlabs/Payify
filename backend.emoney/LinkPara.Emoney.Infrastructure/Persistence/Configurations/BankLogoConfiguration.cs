using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class BankLogoConfiguration : IEntityTypeConfiguration<BankLogo>
{
    public void Configure(EntityTypeBuilder<BankLogo> builder)
    {
        builder.Property(t => t.ContentType).HasMaxLength(100).IsRequired();
        builder.HasIndex(t => t.BankId);
    }
}
