using LinkPara.Fraud.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Fraud.Infrastructure.Persistence.Configurations;

public class SearchLogConfiguration : IEntityTypeConfiguration<SearchLog>
{
    public void Configure(EntityTypeBuilder<SearchLog> builder)
    {
        builder.Property(b => b.SearchName).IsRequired().HasMaxLength(200);
        builder.Property(b => b.BlacklistName).HasMaxLength(500);
        builder.Property(b => b.ChannelType).HasMaxLength(200);
        builder.Property(b => b.BirthYear).HasMaxLength(10);
        builder.Property(b => b.MatchRate).IsRequired();
        builder.Property(b => b.ExpireDate).IsRequired(false);
        builder.Property(b => b.ClientIpAddress).HasMaxLength(50);
        builder.Property(b => b.ReferenceNumber).HasMaxLength(50);
    }
}
