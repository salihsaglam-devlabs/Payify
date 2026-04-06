using LinkPara.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Identity.Infrastructure.Persistence.Configurations;
public class UserLoginLastActivityConfiguration : IEntityTypeConfiguration<UserLoginLastActivity>
{
    public void Configure(EntityTypeBuilder<UserLoginLastActivity> builder)
    {
        builder.Property(t => t.Channel).HasMaxLength(300);

        builder.HasOne(a => a.User).WithOne(u => u.LoginLastActivity)
            .HasForeignKey<UserLoginLastActivity>(a => a.UserId)
            .IsRequired(true);

        builder.HasIndex(u => u.UserId).IsUnique();
    }
}
