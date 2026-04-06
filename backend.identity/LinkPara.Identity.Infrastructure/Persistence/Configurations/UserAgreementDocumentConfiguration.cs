using LinkPara.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Identity.Infrastructure.Persistence.Configurations;

public class UserAgreementDocumentConfiguration : IEntityTypeConfiguration<UserAgreementDocument>
{
    public void Configure(EntityTypeBuilder<UserAgreementDocument> builder)
    {
        builder.HasIndex(u => u.AgreementDocumentVersionId);
        builder.HasIndex(u => u.UserId);
        builder.Property(u => u.ApprovalChannel)
            .HasMaxLength(50);
    }
}