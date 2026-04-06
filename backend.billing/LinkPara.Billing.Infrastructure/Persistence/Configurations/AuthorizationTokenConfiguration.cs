using LinkPara.Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Billing.Infrastructure.Persistence.Configurations;

public class AuthorizationTokenConfiguration : IEntityTypeConfiguration<AuthorizationToken>
{
    public void Configure(EntityTypeBuilder<AuthorizationToken> builder)
    {
        builder.Property(s => s.AccessToken).IsRequired();
        builder.Property(s => s.RefreshToken).IsRequired();
        builder.Property(s => s.TokenType).HasMaxLength(50).IsRequired();
        builder.Property(s => s.ExpiryDate).IsRequired();
        builder.Property(s => s.RegisterDate).IsRequired();
        builder.Property(s => s.VendorId).IsRequired();

        builder.HasIndex(s => new { s.VendorId, s.ExpiryDate });
    }
}