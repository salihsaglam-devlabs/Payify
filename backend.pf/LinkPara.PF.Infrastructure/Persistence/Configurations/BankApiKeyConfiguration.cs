using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class BankApiKeyConfiguration : IEntityTypeConfiguration<BankApiKey>
{
    public void Configure(EntityTypeBuilder<BankApiKey> builder)
    {
        builder.Property(b => b.Key).HasMaxLength(50);
        builder.Property(b => b.MappingName).HasMaxLength(50);
        builder.Property(b => b.AcquireBankId).IsRequired();
        builder.Property(b => b.Category).IsRequired();
    }
}
