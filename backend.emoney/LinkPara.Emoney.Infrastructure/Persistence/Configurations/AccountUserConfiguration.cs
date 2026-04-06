using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class AccountUserConfiguration : IEntityTypeConfiguration<AccountUser>
{
    public void Configure(EntityTypeBuilder<AccountUser> builder)
    {
        builder.Property(s => s.Firstname).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Lastname).IsRequired().HasMaxLength(100);
        builder.Property(s => s.PhoneCode).HasMaxLength(10);
        builder.Property(s => s.PhoneNumber).HasMaxLength(50);
        builder.Property(s => s.Email).HasMaxLength(256);
    }
}
