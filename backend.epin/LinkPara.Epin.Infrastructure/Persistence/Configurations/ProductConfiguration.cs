using LinkPara.Epin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace LinkPara.Epin.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(s => s.Name).HasMaxLength(300).IsRequired();
        builder.Property(t => t.Price).HasPrecision(18, 4);
        builder.Property(t => t.UnitPrice).HasPrecision(18, 4);
        builder.Property(s => s.Equivalent).HasMaxLength(300).IsRequired();
        builder.Property(s => s.Vat).HasMaxLength(100).IsRequired();
        builder.Property(t => t.Discount).HasPrecision(18, 4);

        builder
         .HasOne(s => s.Publisher)
         .WithMany()
         .HasForeignKey(s => s.PublisherId)
         .HasPrincipalKey(s => s.Id)
         .OnDelete(DeleteBehavior.Restrict);

        builder
        .HasOne(s => s.Brand)
        .WithMany()
        .HasForeignKey(s => s.BrandId)
        .HasPrincipalKey(s => s.Id)
        .OnDelete(DeleteBehavior.Restrict);
    }
}
