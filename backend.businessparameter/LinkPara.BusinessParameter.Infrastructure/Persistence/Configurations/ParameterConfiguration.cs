using LinkPara.BusinessParameter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.BusinessParameter.Infrastructure.Persistence.Configurations;

public class ParameterConfiguration : IEntityTypeConfiguration<Parameter>
{
    public void Configure(EntityTypeBuilder<Parameter> builder)
    {
        builder.Property(x => x.GroupCode).IsRequired().HasMaxLength(100);
        builder.Property(x => x.ParameterCode).IsRequired().HasMaxLength(100);
        builder.Property(x => x.ParameterValue).IsRequired().HasMaxLength(100);

        builder.HasIndex(c => new { c.GroupCode, c.ParameterCode }).IsUnique();

        builder
          .HasOne(s => s.ParameterGroup)
          .WithMany()
          .HasForeignKey(s => s.GroupCode)
          .HasPrincipalKey(s => s.GroupCode)
          .OnDelete(DeleteBehavior.Restrict);
    }
}
