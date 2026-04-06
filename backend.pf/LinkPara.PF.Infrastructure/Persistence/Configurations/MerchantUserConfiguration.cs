using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class MerchantUserConfiguration : IEntityTypeConfiguration<MerchantUser>
{
    public void Configure(EntityTypeBuilder<MerchantUser> builder)
    {
        builder.Property(b => b.UserId).IsRequired();
        builder.Property(b => b.Name).IsRequired().HasMaxLength(100);
        builder.Property(b => b.Surname).IsRequired().HasMaxLength(100);
        builder.Property(b => b.BirthDate).IsRequired();
        builder.Property(b => b.Email).IsRequired().HasMaxLength(100);
        builder.Property(b => b.MobilePhoneNumber).IsRequired().HasMaxLength(20);
        builder.Property(b => b.RoleId).HasMaxLength(50);
        builder.Property(b => b.RoleName).HasMaxLength(150);
        builder.Property(b => b.MerchantId).IsRequired();
        builder.Property(b => b.ExternalPersonId).HasDefaultValue(Guid.Empty);
    }
}
