using LinkPara.BusinessParameter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.BusinessParameter.Infrastructure.Persistence.Configurations;

public class ParameterGroupConfiguration : IEntityTypeConfiguration<ParameterGroup>
{
    public void Configure(EntityTypeBuilder<ParameterGroup> builder)
    {
        builder.Property(x => x.GroupCode).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Explanation).IsRequired().HasMaxLength(300);

        builder.HasIndex(c => c.GroupCode).IsUnique();
    }
}
