using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class CardBinConfiguration : IEntityTypeConfiguration<CardBin>
{
    public void Configure(EntityTypeBuilder<CardBin> builder)
    {
        builder.Property(b => b.BinNumber).IsRequired().HasMaxLength(10);
        builder.Property(b => b.CardBrand).IsRequired();
        builder.Property(b => b.Country).IsRequired();
        builder.Property(b => b.CountryName).IsRequired().HasMaxLength(200);

        builder
           .HasOne(s => s.Bank)
           .WithMany()
           .HasForeignKey(s => s.BankCode)
           .HasPrincipalKey(s => s.Code)
           .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasIndex(p => p.BinNumber).IsUnique();
    }
}
