using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class MerchantCounterConfiguration : IEntityTypeConfiguration<MerchantCounter>
{
    public void Configure(EntityTypeBuilder<MerchantCounter> builder)
    {
        builder.HasIndex(s => s.NumberCounter).IsUnique();
        builder.Property(s => s.NumberCounter).ValueGeneratedOnAdd();
        builder.Property(s => s.NumberCounter).IsRequired();
    }
}
