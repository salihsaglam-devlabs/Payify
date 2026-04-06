using LinkPara.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Identity.Infrastructure.Persistence.Configurations;
public class LoginActivityConfiguration : IEntityTypeConfiguration<LoginActivity>
{
    public void Configure(EntityTypeBuilder<LoginActivity> builder)
    {
        builder.Property(t => t.Channel).HasMaxLength(300);

        builder
            .HasOne(s => s.User)
            .WithMany(s => s.LoginActivity)
            .HasForeignKey(s => s.UserId);
    }
}
