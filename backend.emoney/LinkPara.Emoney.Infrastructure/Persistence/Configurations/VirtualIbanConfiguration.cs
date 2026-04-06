using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class VirtualIbanConfiguration : IEntityTypeConfiguration<VirtualIban>
{
    public void Configure(EntityTypeBuilder<VirtualIban> builder)
    {
        builder.Property(s => s.Iban)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(s => s.BankCode)
            .IsRequired();

        builder.HasIndex(s => s.Iban).IsUnique();
    }
}
