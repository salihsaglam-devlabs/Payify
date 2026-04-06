using LinkPara.Approval.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Approval.Infrastructure.Persistence.Configurations;

public class MakerCheckerConfiguration : IEntityTypeConfiguration<MakerChecker>
{
    public void Configure(EntityTypeBuilder<MakerChecker> builder)
    {
        builder.HasIndex(t => t.RecordStatus);
        builder.HasIndex(t => t.CaseId);
    }
}
