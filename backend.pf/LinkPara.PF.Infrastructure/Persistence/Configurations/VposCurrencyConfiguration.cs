using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class VposCurrencyConfiguration : IEntityTypeConfiguration<VposCurrency>
{
    public void Configure(EntityTypeBuilder<VposCurrency> builder)
    {
        builder.Property(b => b.VposId).IsRequired();

        builder
           .HasOne(b => b.Currency)
           .WithMany()
           .HasForeignKey(b => b.CurrencyCode)
           .HasPrincipalKey(b => b.Code)
           .OnDelete(DeleteBehavior.Restrict);
    }
}
