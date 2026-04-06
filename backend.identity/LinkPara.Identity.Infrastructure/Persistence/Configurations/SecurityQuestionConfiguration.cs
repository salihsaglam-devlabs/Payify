using LinkPara.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Identity.Infrastructure.Persistence.Configurations;

public class SecurityQuestionConfiguration : IEntityTypeConfiguration<SecurityQuestion>
{
    public void Configure(EntityTypeBuilder<SecurityQuestion> builder)
    {
        builder.Property(t => t.Question).HasMaxLength(100).IsRequired();
        builder.Property(t => t.LanguageCode).HasMaxLength(10).IsRequired();
    }
}