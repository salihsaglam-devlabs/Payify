using LinkPara.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Identity.Infrastructure.Persistence.Configurations;

public class LoginWhitelistConfiguration : IEntityTypeConfiguration<LoginWhitelist>
{
    public void Configure(EntityTypeBuilder<LoginWhitelist> builder)
    {
        builder.HasIndex(u => u.PhoneNumber).IsUnique();
        builder.Property(u => u.PhoneNumber).HasMaxLength(50).IsRequired();
        builder.Property(u => u.PhoneCode).HasMaxLength(10).IsRequired();
        builder.Property(u => u.FirstName).HasMaxLength(50).IsRequired();
        builder.Property(u => u.LastName).HasMaxLength(50).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(50);        
    }
}