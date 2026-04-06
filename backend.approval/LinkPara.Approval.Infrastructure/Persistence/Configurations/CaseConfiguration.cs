using LinkPara.Approval.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Approval.Infrastructure.Persistence.Configurations;

public class CaseConfiguration : IEntityTypeConfiguration<Case>
{
    public void Configure(EntityTypeBuilder<Case> builder)
    {
        builder.Property(t => t.BaseUrl).IsRequired().HasMaxLength(100);
        builder.Property(t => t.DisplayName).IsRequired().HasMaxLength(100);
        builder.Property(t => t.ModuleName).IsRequired().HasMaxLength(100);
        builder.Property(t => t.ActionName).HasMaxLength(100);
        builder.HasIndex(t => t.RecordStatus);
        builder.HasIndex(t => t.CreateDate);

        builder
            .HasMany(b => b.MakerCheckers)
            .WithOne(b => b.Case)
            .IsRequired();
    }
}
