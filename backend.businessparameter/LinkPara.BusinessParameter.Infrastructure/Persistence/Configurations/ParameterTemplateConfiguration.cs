using LinkPara.BusinessParameter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.BusinessParameter.Infrastructure.Persistence.Configurations;

public class ParameterTemplateConfiguration : IEntityTypeConfiguration<ParameterTemplate>
{
    public void Configure(EntityTypeBuilder<ParameterTemplate> builder)
    {
        builder.Property(x => x.GroupCode).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Explanation).IsRequired().HasMaxLength(300);
        builder.Property(x => x.DataType).IsRequired();
        builder.Property(x => x.TemplateCode).IsRequired().HasMaxLength(100);

        builder.HasIndex(c => new { c.GroupCode, c.TemplateCode }).IsUnique();

        builder
          .HasOne(s => s.ParameterGroup)
          .WithMany()
          .HasForeignKey(s => s.GroupCode)
          .HasPrincipalKey(s => s.GroupCode)
          .OnDelete(DeleteBehavior.Restrict);
    }
}
