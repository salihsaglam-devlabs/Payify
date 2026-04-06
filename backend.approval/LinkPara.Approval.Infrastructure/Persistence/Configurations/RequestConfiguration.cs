using LinkPara.Approval.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Approval.Infrastructure.Persistence.Configurations;

public class RequestConfiguration : IEntityTypeConfiguration<Request>
{
    public void Configure(EntityTypeBuilder<Request> builder)
    {
        builder.Property(t => t.Url).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Resource).IsRequired().HasMaxLength(100);
        builder.Property(t => t.DisplayName).IsRequired().HasMaxLength(100);
        builder.Property(t => t.QueryParameters).HasMaxLength(1500);
        builder.Property(t => t.Body);
        builder.Property(t => t.Status).IsRequired();
        builder.Property(t => t.ActionType).IsRequired();
        builder.Property(t => t.MakerFullName).HasMaxLength(100);
        builder.Property(t => t.CheckerFullName).HasMaxLength(100);
        builder.Property(t => t.SecondCheckerFullName).HasMaxLength(100);
        builder.Property(t => t.Reason).HasMaxLength(1500);
        builder.Property(t => t.MakerDescription).HasMaxLength(1500);
        builder.Property(t => t.FirstApproverDescription).HasMaxLength(1500);
        builder.Property(t => t.SecondApproverDescription).HasMaxLength(1500);
        builder.Property(t => t.ExceptionMessage).HasMaxLength(4001);
        builder.Property(t => t.ExceptionDetails).HasMaxLength(4001);
        builder.Property(t => t.ReferenceId).UseIdentityColumn()
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        builder.Property(t => t.ModuleName).IsRequired().HasMaxLength(100).HasDefaultValue(string.Empty);

        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.UpdateDate);
        builder.HasIndex(t => t.OperationType);
        builder.HasIndex(t => t.ReferenceId);
    }
}
