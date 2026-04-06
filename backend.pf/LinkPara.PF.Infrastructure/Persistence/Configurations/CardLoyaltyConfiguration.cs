using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class CardLoyaltyConfiguration : IEntityTypeConfiguration<CardLoyalty>
{
    public void Configure(EntityTypeBuilder<CardLoyalty> builder)
    {
        builder.Property(b => b.Name).IsRequired().HasMaxLength(50);
        
        builder
            .HasOne(s => s.Bank)
            .WithMany()
            .HasForeignKey(s => s.BankCode)
            .HasPrincipalKey(s => s.Code)
            .OnDelete(DeleteBehavior.Restrict);
    }
}