using LinkPara.Kkb.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Kkb.Infrastructure.Persistence.Configurations
{
    public class KkbAuthorizationTokenConfiguration : IEntityTypeConfiguration<KkbAuthorizationToken>
    {
        public void Configure(EntityTypeBuilder<KkbAuthorizationToken> builder)
        {
            builder.Property(t => t.AccessToken);
            builder.Property(t => t.RefreshToken);
            builder.Property(t => t.TokenType).HasMaxLength(30);
            builder.Property(t => t.ExpiresDate);
            builder.Property(t => t.IsSuccess).IsRequired();
            builder.Property(t => t.Error).HasMaxLength(30);
            builder.Property(t => t.ErrorDescription);
        }
    }
}
