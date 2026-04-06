using LinkPara.Accounting.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Accounting.Infrastructure.Persistence.Configurations;

public class TemplateConfiguration : IEntityTypeConfiguration<Template>
{
    public void Configure(EntityTypeBuilder<Template> builder)
    {
        builder.Property(s => s.TranCode).HasMaxLength(20);
        builder.Property(s => s.Direction).HasMaxLength(5);
        builder.Property(s => s.AccountNumber).HasMaxLength(50);
        builder.Property(s => s.SrvCode).HasMaxLength(50);
    }
}
