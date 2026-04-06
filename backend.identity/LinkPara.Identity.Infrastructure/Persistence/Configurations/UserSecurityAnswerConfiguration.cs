using LinkPara.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Identity.Infrastructure.Persistence.Configurations;

public class UserSecurityAnswerConfiguration : IEntityTypeConfiguration<UserSecurityAnswer>
{
    public void Configure(EntityTypeBuilder<UserSecurityAnswer> builder)
    {
        builder.Property(u => u.AnswerHash).HasMaxLength(400).IsRequired();
        builder.Property(u => u.UserId).HasMaxLength(450);
        builder.HasIndex(u => u.SecurityQuestionId);
        builder.HasIndex(u => u.UserId);
    }
}