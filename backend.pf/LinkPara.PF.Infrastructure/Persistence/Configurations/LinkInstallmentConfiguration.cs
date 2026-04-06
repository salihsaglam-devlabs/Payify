using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;


namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class LinkInstallmentConfiguration : IEntityTypeConfiguration<LinkInstallment>
{
    public void Configure(EntityTypeBuilder<LinkInstallment> builder)
    {
        builder
          .HasOne(b => b.Link)
          .WithMany(b => b.LinkInstallments)
          .HasForeignKey(c => c.LinkId)
          .OnDelete(DeleteBehavior.Restrict);
    }
}
