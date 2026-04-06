using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class MccConfiguration : IEntityTypeConfiguration<Mcc>
{
    public void Configure(EntityTypeBuilder<Mcc> builder)
    {
        builder.Property(b => b.Code).IsRequired().HasMaxLength(4);
        builder.Property(b => b.Name).IsRequired().HasMaxLength(256);
        builder.Property(b => b.Description).HasMaxLength(300);
    }
}
