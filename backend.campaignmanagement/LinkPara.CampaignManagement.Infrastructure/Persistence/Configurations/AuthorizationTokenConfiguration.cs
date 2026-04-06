using LinkPara.CampaignManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.CampaignManagement.Infrastructure.Persistence.Configurations
{
    public class AuthorizationTokenConfiguration : IEntityTypeConfiguration<AuthorizationToken>
    {
        public void Configure(EntityTypeBuilder<AuthorizationToken> builder)
        {
            builder.Property(s => s.Token).HasMaxLength(1000).IsRequired();
            builder.Property(s => s.RefreshTokenDate).IsRequired();
            builder.Property(s => s.ExpiryDate).IsRequired();

            builder.HasIndex(s => s.RefreshTokenDate);
            builder.HasIndex(s => s.ExpiryDate);
        }
    }
}
