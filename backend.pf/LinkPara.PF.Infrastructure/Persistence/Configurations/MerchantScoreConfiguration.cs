using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class MerchantScoreConfiguration : IEntityTypeConfiguration<MerchantScore>
{
    public void Configure(EntityTypeBuilder<MerchantScore> builder)
    {
        builder.Property(b => b.AlexaRank).HasMaxLength(10);
        builder.Property(b => b.GoogleRank).HasMaxLength(10);
    }
}
