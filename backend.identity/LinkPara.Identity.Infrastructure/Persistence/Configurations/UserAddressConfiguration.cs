using LinkPara.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Identity.Infrastructure.Persistence.Configurations;

public class UserAddressConfiguration : IEntityTypeConfiguration<UserAddress>
{
    public void Configure(EntityTypeBuilder<UserAddress> builder)
    {
        builder.Property(u => u.Street).HasMaxLength(450).IsRequired();
        builder.Property(u => u.FullAddress).HasMaxLength(600).IsRequired();
        builder.Property(u => u.Neighbourhood).HasMaxLength(450).IsRequired();
        builder.Property(u => u.LastModifiedBy).HasMaxLength(100);
        builder.HasIndex(u => u.UserId);
    }
}