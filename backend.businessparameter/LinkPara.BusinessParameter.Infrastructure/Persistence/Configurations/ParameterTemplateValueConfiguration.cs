using LinkPara.BusinessParameter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.BusinessParameter.Infrastructure.Persistence.Configurations;

public class ParameterTemplateValueConfiguration : IEntityTypeConfiguration<ParameterTemplateValue>
{
    public void Configure(EntityTypeBuilder<ParameterTemplateValue> builder)
    {
        builder.Property(x => x.GroupCode).IsRequired().HasMaxLength(100);
        builder.Property(x => x.ParameterCode).IsRequired().HasMaxLength(100);
        builder.Property(x => x.TemplateCode).IsRequired().HasMaxLength(100);
        builder.Property(x => x.TemplateValue).IsRequired().HasMaxLength(256);

        builder.HasIndex(c => new { c.GroupCode, c.TemplateCode, c.ParameterCode }).IsUnique();

        builder
          .HasOne(s => s.ParameterGroup)
          .WithMany()
          .HasForeignKey(s => s.GroupCode)
          .HasPrincipalKey(s => s.GroupCode)
          .OnDelete(DeleteBehavior.Restrict);

        builder
         .HasOne(s => s.Parameter)
         .WithMany()
         .HasForeignKey(s => new { s.ParameterCode, s.GroupCode })
         .HasPrincipalKey(s => new { s.ParameterCode, s.GroupCode })
         .OnDelete(DeleteBehavior.Restrict);
    }
}
