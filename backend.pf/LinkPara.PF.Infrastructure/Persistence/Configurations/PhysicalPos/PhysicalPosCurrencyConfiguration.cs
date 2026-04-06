using LinkPara.PF.Domain.Entities.PhysicalPos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations.PhysicalPos;

public class PhysicalPosCurrencyConfiguration : IEntityTypeConfiguration<PhysicalPosCurrency>
{
    public void Configure(EntityTypeBuilder<PhysicalPosCurrency> builder)
    {
        builder.Property(b => b.PhysicalPosId).IsRequired();

        builder
           .HasOne(b => b.Currency)
           .WithMany()
           .HasForeignKey(b => b.CurrencyCode)
           .HasPrincipalKey(b => b.Code)
           .OnDelete(DeleteBehavior.Restrict);
    }
}
