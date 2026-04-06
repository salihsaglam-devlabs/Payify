

using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class LinkCustomerConfiguration : IEntityTypeConfiguration<LinkCustomer>
{
    public void Configure(EntityTypeBuilder<LinkCustomer> builder)
    {
        builder.HasIndex(u => u.LinkTransactionId);

        builder.Property(b => b.Name).HasMaxLength(100);
        builder.Property(b => b.Email).HasMaxLength(100);
        builder.Property(u => u.PhoneNumber).HasMaxLength(30);
        builder.Property(b => b.Address).HasMaxLength(256);
        builder.Property(b => b.Note).HasMaxLength(256);
    }
}
